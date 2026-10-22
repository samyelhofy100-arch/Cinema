using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Cinema.Areas.Client.Controllers
{
    [Area("Client")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context = new ApplicationDbContext();
        int PageSize = 8;

        public IActionResult Index(string? search, int? categoryId, int? cinemaId, int? actorId ,int page=1)
        {
            var movies = context.Movies
                .Include(m => m.Category).Include(m => m.Cinema).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                movies = movies.Where(m => m.Name.Contains(search));

            if (categoryId.HasValue)
                movies = movies.Where(m => m.CategoryId == categoryId);

            if (cinemaId.HasValue)
                movies = movies.Where(m => m.CinemaId == cinemaId);

            if (actorId.HasValue)
                movies = movies.Where(m => m.MovieActors.Any(ma => ma.ActorId == actorId));
            int totalCount = movies.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var pageItems = movies
                .OrderBy(m => m.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.Categories = context.Categories.ToList();
            ViewBag.CategoryId = categoryId;
            ViewBag.CinemaId = cinemaId;
            ViewBag.ActorId = actorId;
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(pageItems);

        }

        public IActionResult Details(int id)
        {
            var movie = context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .Include(m => m.SubImage)
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        public IActionResult Cinemas()
        {
            var cinemas = context.Cinemas.Include(c => c.Movies).ToList();
            return View(cinemas);
        }

        public IActionResult Actors()
        {
            var actors = context.Actors.Include(a => a.MovieActors).ToList();
            return View(actors);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
