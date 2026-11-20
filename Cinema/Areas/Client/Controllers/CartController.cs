using System.Security.Claims;
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize]
    public class CartController : Controller
    {
        private const int MaxPerMovie = 10;

        private readonly ApplicationDbContext context;

        public CartController(ApplicationDbContext context)
        {
            this.context = context;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private Task<Cart?> FindItem(int id) =>
            context.Carts.FirstOrDefaultAsync(c => c.Id == id && c.ApplicationUserId == UserId);

        // ---------- عرض السلة ----------
        public async Task<IActionResult> Index()
        {
            var items = await context.Carts
                .Include(c => c.Movie).ThenInclude(m => m.Cinema)
                .Where(c => c.ApplicationUserId == UserId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            ViewBag.Total = items.Sum(c => c.Movie.Price * c.Count);
            return View(items);
        }

        // ---------- إضافة ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int movieId, int count = 1)
        {
            var movie = await context.Movies.FindAsync(movieId);
            if (movie == null) return NotFound();

            if (!movie.Status)
            {
                TempData["Error"] = "This movie is not available right now.";
                return RedirectToAction("Details", "Home", new { id = movieId });
            }

            count = Math.Clamp(count, 1, MaxPerMovie);

            var item = await context.Carts
                .FirstOrDefaultAsync(c => c.ApplicationUserId == UserId && c.MovieId == movieId);

            if (item == null)
                context.Carts.Add(new Cart { ApplicationUserId = UserId, MovieId = movieId, Count = count });
            else
                item.Count = Math.Min(MaxPerMovie, item.Count + count);

            await context.SaveChangesAsync();

            TempData["Success"] = $"\"{movie.Name}\" was added to your cart.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- عمليات السلة ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Increment(int id)
        {
            var item = await FindItem(id);
            if (item == null) return NotFound();

            if (item.Count < MaxPerMovie)
            {
                item.Count++;
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrement(int id)
        {
            var item = await FindItem(id);
            if (item == null) return NotFound();

            if (item.Count > 1)
            {
                item.Count--;
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await FindItem(id);
            if (item == null) return NotFound();

            context.Carts.Remove(item);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            await context.Carts.Where(c => c.ApplicationUserId == UserId).ExecuteDeleteAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}