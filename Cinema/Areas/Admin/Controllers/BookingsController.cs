using Cinema.Data;
using Cinema.Helpers;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRoles)]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext context;
        public BookingsController(ApplicationDbContext context) { this.context = context; }

        public async Task<IActionResult> Index()
        {
            var orders = await context.Orders
                .Include(o => o.User)
                .Include(o => o.Tickets).ThenInclude(t => t.Seat)
                .Include(o => o.Tickets).ThenInclude(t => t.Showtime).ThenInclude(s => s.Movie)
                .Where(o => o.Status == OrderStatus.Paid)
                .OrderByDescending(o => o.CreatedAt)
                .Take(200)
                .ToListAsync();

            return View(orders);
        }
    }
}