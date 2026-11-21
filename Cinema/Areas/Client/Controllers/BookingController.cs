using System.Security.Claims;
using Cinema.Data;
using Cinema.Helpers;
using Cinema.Models;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly BookingService booking;

        public BookingController(ApplicationDbContext db, BookingService booking)
        {
            this.db = db;
            this.booking = booking;
        }

        private string Uid => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> Seats(int showtimeId)
        {
            await booking.CleanupAsync();

            var showtime = await db.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall).ThenInclude(h => h.Cinema)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null) return NotFound();
            if (showtime.StartTime <= DateTime.Now)
            {
                TempData["BookingError"] = "This showtime has already started.";
                return RedirectToAction("Details", "Home", new { area = "Client", id = showtime.MovieId });
            }

            var seats = await db.Seats.Where(s => s.HallId == showtime.HallId)
                .OrderBy(s => s.Row).ThenBy(s => s.Number).ToListAsync();

            var sold = await db.Tickets.Where(t => t.ShowtimeId == showtimeId)
                .Select(t => t.SeatId).ToListAsync();

            var held = await db.CartItems.Where(c => c.ShowtimeId == showtimeId)
                .Select(c => new { c.SeatId, c.UserId }).ToListAsync();

            var vm = new SeatMapVM
            {
                Showtime = showtime,
                Seats = seats,
                Mine = held.Where(h => h.UserId == Uid).Select(h => h.SeatId).ToHashSet(),
                Taken = sold.Concat(held.Where(h => h.UserId != Uid).Select(h => h.SeatId)).ToHashSet()
            };

            return View(vm);
        }

        // الاختيار اللي في الفورم هو الاختيار النهائي للموعد ده (بيستبدل القديم)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Hold(int showtimeId, List<int>? seatIds)
        {
            await booking.CleanupAsync();
            seatIds = (seatIds ?? new()).Distinct().ToList();

            var showtime = await db.Showtimes.FirstOrDefaultAsync(s => s.Id == showtimeId);
            if (showtime == null) return NotFound();

            if (showtime.StartTime <= DateTime.Now)
            {
                TempData["BookingError"] = "This showtime has already started.";
                return RedirectToAction("Details", "Home", new { area = "Client", id = showtime.MovieId });
            }

            if (seatIds.Count > BookingService.MaxSeats)
            {
                TempData["BookingError"] = $"You can book up to {BookingService.MaxSeats} seats per showtime.";
                return RedirectToAction(nameof(Seats), new { showtimeId });
            }

            var validCount = await db.Seats.CountAsync(s => s.HallId == showtime.HallId && seatIds.Contains(s.Id));
            if (validCount != seatIds.Count) return BadRequest();

            bool soldAlready = await db.Tickets.AnyAsync(t => t.ShowtimeId == showtimeId && seatIds.Contains(t.SeatId));
            bool heldByOthers = await db.CartItems.AnyAsync(c =>
                c.ShowtimeId == showtimeId && c.UserId != Uid && seatIds.Contains(c.SeatId));

            if (soldAlready || heldByOthers)
            {
                TempData["BookingError"] = "Some of the seats you picked were just taken. Please choose again.";
                return RedirectToAction(nameof(Seats), new { showtimeId });
            }

            var expires = DateTime.Now.AddMinutes(BookingService.HoldMinutes);
            var mine = await db.CartItems.Where(c => c.ShowtimeId == showtimeId && c.UserId == Uid).ToListAsync();

            db.CartItems.RemoveRange(mine.Where(c => !seatIds.Contains(c.SeatId)));

            foreach (var item in mine.Where(c => seatIds.Contains(c.SeatId)))
                item.ExpiresAt = expires;

            var existing = mine.Select(c => c.SeatId).ToHashSet();
            foreach (var seatId in seatIds.Where(id => !existing.Contains(id)))
                db.CartItems.Add(new CartItem { UserId = Uid, ShowtimeId = showtimeId, SeatId = seatId, ExpiresAt = expires });

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["BookingError"] = "Someone just took one of these seats. Please choose again.";
                return RedirectToAction(nameof(Seats), new { showtimeId });
            }

            return seatIds.Count == 0
                ? RedirectToAction(nameof(Seats), new { showtimeId })
                : RedirectToAction("Index", "Cart", new { area = "Client" });
        }
    }
}