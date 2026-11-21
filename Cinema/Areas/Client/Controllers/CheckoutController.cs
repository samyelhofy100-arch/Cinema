
using System.Security.Claims;
using Cinema.Data;
using Cinema.Helpers;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace Cinema.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly BookingService booking;

        public CheckoutController(
            ApplicationDbContext db,
            BookingService booking)
        {
            this.db = db;
            this.booking = booking;
        }

        private string Uid =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay()
        {
            await booking.CleanupAsync();

            var now = DateTime.Now;

            var items = await db.CartItems
                .Include(c => c.Seat)
                .Include(c => c.Showtime).ThenInclude(s => s.Movie)
                .Include(c => c.Showtime).ThenInclude(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .Where(c => c.UserId == Uid)
                .ToListAsync();

            if (items.Count == 0)
            {
                TempData["BookingError"] =
                    "Your cart is empty or your seats expired.";

                return RedirectToAction("Index", "Cart", new { area = "Client" });
            }

            if (items.Any(i => i.Showtime.StartTime <= now))
            {
                db.CartItems.RemoveRange(
                    items.Where(i => i.Showtime.StartTime <= now)
                );

                await db.SaveChangesAsync();

                TempData["BookingError"] =
                    "Some showtimes have already started and were removed from your cart.";

                return RedirectToAction("Index", "Cart", new { area = "Client" });
            }

            var order = new Order
            {
                UserId = Uid,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(BookingService.PaymentMinutes),
                Status = OrderStatus.Pending,
                TotalAmount = items.Sum(i => i.Showtime.Price),

                Tickets = items.Select(i => new Ticket
                {
                    ShowtimeId = i.ShowtimeId,
                    SeatId = i.SeatId,
                    Price = i.Showtime.Price,
                    Code = Guid.NewGuid().ToString("N")[..8].ToUpper()
                }).ToList()
            };

            db.Orders.Add(order);
            db.CartItems.RemoveRange(items);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["BookingError"] =
                    "Some seats are no longer available. Please check your cart.";

                return RedirectToAction("Index", "Cart", new { area = "Client" });
            }

            try
            {
                var lines = items
                    .GroupBy(i => i.ShowtimeId)
                    .Select(g =>
                    {
                        var st = g.First().Showtime;

                        return new SessionLineItemOptions
                        {
                            Quantity = g.Count(),

                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "egp",
                                UnitAmount = (long)Math.Round(
                                    st.Price * 100,
                                    MidpointRounding.AwayFromZero
                                ),

                                ProductData =
                                    new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = $"{st.Movie.Name} - {st.Hall.Cinema.Name} / {st.Hall.Name} - {st.StartTime:ddd dd MMM HH:mm}",

                                        Description = "Seats: " +
                                            string.Join(", ", g.Select(x => x.Seat.Label))
                                    }
                            }
                        };
                    })
                    .ToList();

                var successUrl =
                    Url.Action(
                        "Success",
                        "Checkout",
                        new
                        {
                            area = "Client",
                            orderId = order.Id
                        },
                        Request.Scheme
                    ) + "&session_id={CHECKOUT_SESSION_ID}";

                var options = new SessionCreateOptions
                {
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = Url.Action(
                        "Cancel",
                        "Checkout",
                        new
                        {
                            area = "Client",
                            orderId = order.Id
                        },
                        Request.Scheme
                    ),

                    ClientReferenceId = order.Id.ToString(),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(31),
                    LineItems = lines
                };

                var session = await new SessionService().CreateAsync(options);

                order.StripeSessionId = session.Id;

                await db.SaveChangesAsync();

                return Redirect(session.Url);
            }
            catch (Exception)
            {
                await booking.CancelPendingAsync(order);

                var expires = DateTime.Now.AddMinutes(
                    BookingService.HoldMinutes
                );

                foreach (var i in items)
                {
                    db.CartItems.Add(new CartItem
                    {
                        UserId = Uid,
                        ShowtimeId = i.ShowtimeId,
                        SeatId = i.SeatId,
                        ExpiresAt = expires
                    });
                }

                await db.SaveChangesAsync();

                TempData["BookingError"] =
                    "Payment could not be started. Please try again.";

                return RedirectToAction("Index", "Cart", new { area = "Client" });
            }
        }

        public async Task<IActionResult> Success(
            int orderId,
            string session_id)
        {
            var order = await LoadOrder(orderId);

            if (order == null)
                return NotFound();

            if (order.Status == OrderStatus.Pending)
            {
                var session = await new SessionService().GetAsync(session_id);

                if (session.Id == order.StripeSessionId &&
                    session.PaymentStatus == "paid")
                {
                    order.Status = OrderStatus.Paid;
                    await db.SaveChangesAsync();
                }
            }

            return View(order);
        }

        public async Task<IActionResult> Cancel(int orderId)
        {
            var order = await db.Orders
                .FirstOrDefaultAsync(
                    o => o.Id == orderId && o.UserId == Uid
                );

            if (order != null && order.Status == OrderStatus.Pending)
            {
                await booking.CancelPendingAsync(order);

                if (!string.IsNullOrEmpty(order.StripeSessionId))
                {
                    try
                    {
                        await new SessionService().ExpireAsync(
                            order.StripeSessionId
                        );
                    }
                    catch
                    {
                    }
                }
            }

            TempData["BookingError"] =
                "Payment was cancelled. Your seats were released.";

            return RedirectToAction("Index", "Cart", new { area = "Client" });
        }

        public async Task<IActionResult> Orders()
        {
            await booking.CleanupAsync();

            var orders = await db.Orders
                .Include(o => o.Tickets).ThenInclude(t => t.Seat)
                .Include(o => o.Tickets).ThenInclude(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(o => o.Tickets).ThenInclude(t => t.Showtime)
                    .ThenInclude(s => s.Hall).ThenInclude(h => h.Cinema)
                .Where(o => o.UserId == Uid && o.Status == OrderStatus.Paid)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        private Task<Order?> LoadOrder(int id) =>
            db.Orders
                .Include(o => o.Tickets).ThenInclude(t => t.Seat)
                .Include(o => o.Tickets).ThenInclude(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(o => o.Tickets).ThenInclude(t => t.Showtime)
                    .ThenInclude(s => s.Hall).ThenInclude(h => h.Cinema)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == Uid);
    }
}
