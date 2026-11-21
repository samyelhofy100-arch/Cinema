using System.Security.Claims;
using Cinema.Data;
using Cinema.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly BookingService booking;

        public CartController(ApplicationDbContext db, BookingService booking)
        {
            this.db = db;
            this.booking = booking;
        }

        private string Uid => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            await booking.CleanupAsync();

            var items = await db.CartItems
                .Include(c => c.Seat)
                .Include(c => c.Showtime).ThenInclude(s => s.Movie)
                .Include(c => c.Showtime).ThenInclude(s => s.Hall).ThenInclude(h => h.Cinema)
                .Where(c => c.UserId == Uid)
                .OrderBy(c => c.Showtime.StartTime).ThenBy(c => c.Seat.Row).ThenBy(c => c.Seat.Number)
                .ToListAsync();

            return View(items);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            await db.CartItems.Where(c => c.Id == id && c.UserId == Uid).ExecuteDeleteAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            await db.CartItems.Where(c => c.UserId == Uid).ExecuteDeleteAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}