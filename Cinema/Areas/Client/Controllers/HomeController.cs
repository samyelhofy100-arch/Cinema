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
        private readonly ApplicationDbContext context;
        public IActionResult Index(string? search, int? categoryId)
        {
            var movies = context.Movies
                .Include(m => m.Category).Include(m => m.Cinema).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                movies = movies.Where(m => m.Name.Contains(search));

            if (categoryId.HasValue)
                movies = movies.Where(m => m.CategoryId == categoryId);

            ViewBag.Categories = context.Categories.ToList();
            ViewBag.CategoryId = categoryId;
            ViewBag.Search = search;

            return View(movies.ToList());
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
