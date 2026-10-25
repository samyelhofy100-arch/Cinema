using Cinema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public IActionResult Index()
        {
            ViewBag.MoviesCount = _context.Movies.Count();
            ViewBag.AvailableMovies = _context.Movies.Count(m => m.Status);
            ViewBag.ActorsCount = _context.Actors.Count();
            ViewBag.CinemasCount = _context.Cinemas.Count();
            ViewBag.CategoriesCount = _context.Categories.Count();

            var latestMovies = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .OrderByDescending(m => m.DateTime)
                .Take(5)
                .ToList();

            return View(latestMovies);
        }
    }
}