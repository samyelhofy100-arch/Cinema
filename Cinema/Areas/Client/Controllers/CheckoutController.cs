using System.Security.Claims;
using Cinema.Data;
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
        private const string Currency = "egp";   // لو حسابك رفضها غيّرها لـ "usd"

        private readonly ApplicationDbContext context;

        public CheckoutController(ApplicationDbContext context)
        {
            this.context = context;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ---------- 1) ابدأ الدفع ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay()
        {
            var cart = await context.Carts
                .Include(c => c.Movie)
                .Where(c => c.ApplicationUserId == UserId)
                .ToListAsync();

            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (cart.Any(c => !c.Movie.Status))
            {
                TempData["Error"] = "Remove the unavailable movies from your cart first.";
                return RedirectToAction("Index", "Cart");
            }

            // الأسعار بتتاخد من الـ Database، مش من المتصفح
            var order = new Order
            {
                ApplicationUserId = UserId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalPrice = cart.Sum(c => c.Movie.Price * c.Count),
                Items = cart.Select(c => new OrderItem
                {
                    MovieId = c.MovieId,
                    Count = c.Count,
                    Price = c.Movie.Price
                }).ToList()
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                CustomerEmail = User.FindFirstValue(ClaimTypes.Email),
                ClientReferenceId = order.Id.ToString(),
                SuccessUrl = Url.Action("Success", "Checkout", new { area = "Client", orderId = order.Id }, Request.Scheme),
                CancelUrl = Url.Action("Cancel", "Checkout", new { area = "Client", orderId = order.Id }, Request.Scheme),
                LineItems = cart.Select(c => new SessionLineItemOptions
                {
                    Quantity = c.Count,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = Currency,
                        UnitAmount = (long)Math.Round(c.Movie.Price * 100, MidpointRounding.AwayFromZero),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = c.Movie.Name
                        }
                    }
                }).ToList()
            };

            try
            {
                var session = await new SessionService().CreateAsync(options);

                order.SessionId = session.Id;
                await context.SaveChangesAsync();

                return Redirect(session.Url);
            }
            catch (Stripe.StripeException ex)
            {
                order.Status = OrderStatus.Cancelled;
                await context.SaveChangesAsync();

                TempData["Error"] = "Payment service error: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        // ---------- 2) رجع من Stripe بعد الدفع ----------
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Movie)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ApplicationUserId == UserId);

            if (order == null) return NotFound();

            // بنتأكد من Stripe نفسه إن الدفع تم، مش من اللينك
            if (order.Status == OrderStatus.Pending && order.SessionId != null)
            {
                var session = await new SessionService().GetAsync(order.SessionId);

                if (session.PaymentStatus == "paid")
                {
                    order.Status = OrderStatus.Paid;
                    order.PaymentIntentId = session.PaymentIntentId;

                    var movieIds = order.Items.Select(i => i.MovieId).ToList();
                    await context.Carts
                        .Where(c => c.ApplicationUserId == UserId && movieIds.Contains(c.MovieId))
                        .ExecuteDeleteAsync();

                    await context.SaveChangesAsync();
                }
            }

            if (order.Status != OrderStatus.Paid)
            {
                TempData["Error"] = "We couldn't confirm your payment.";
                return RedirectToAction("Index", "Cart");
            }

            return View(order);
        }

        // ---------- 3) لغى الدفع ----------
        public async Task<IActionResult> Cancel(int orderId)
        {
            var order = await context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ApplicationUserId == UserId);

            if (order != null && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Cancelled;
                await context.SaveChangesAsync();
            }

            TempData["Error"] = "Payment was cancelled. Your cart is still here.";
            return RedirectToAction("Index", "Cart");
        }

        // ---------- طلباتي ----------
        public async Task<IActionResult> Orders()
        {
            var orders = await context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Movie)
                .Where(o => o.ApplicationUserId == UserId && o.Status == OrderStatus.Paid)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}