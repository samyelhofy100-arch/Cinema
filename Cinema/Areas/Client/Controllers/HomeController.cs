using System.Diagnostics;
using System.Linq.Expressions;
using Cinema.Models;
using Cinema.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Client.Controllers
{
    [Area("Client")]
    public class HomeController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Cinemas> _cinemaRepository;
        int PageSize = 8;

        public HomeController(
            IRepository<Movie> movieRepository,
            IRepository<Actor> actorRepository,
            IRepository<Category> categoryRepository,
            IRepository<Cinemas> cinemaRepository)
        {
            _movieRepository = movieRepository;
            _actorRepository = actorRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, int? cinemaId, int? actorId, int page = 1)
        {
            Expression<Func<Movie, bool>> filter = m =>
                (string.IsNullOrWhiteSpace(search) || m.Name.Contains(search!)) &&
                (!categoryId.HasValue || m.CategoryId == categoryId) &&
                (!cinemaId.HasValue || m.CinemaId == cinemaId) &&
                (!actorId.HasValue || m.MovieActors.Any(ma => ma.ActorId == actorId));

            int totalCount = await _movieRepository.CountAsync(filter);
            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var pageItems = (await _movieRepository.GetAllAsync(
                filter,
                includes: [m => m.Category, m => m.Cinema, m => m.SubImage],
                orderBy: q => q.OrderBy(m => m.Id),
                skip: (page - 1) * PageSize,
                take: PageSize,
                isTracked: false)).ToList();

            ViewBag.Categories = await _categoryRepository.GetAllAsync();
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

        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieRepository.GetOneAsync(
                m => m.Id == id,
                includes: [m => m.Category, m => m.Cinema, m => m.SubImage],
                includeProperties: ["MovieActors.Actor"]);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        public async Task<IActionResult> Cinemas()
        {
            var cinemas = await _cinemaRepository.GetAllAsync(includes: [c => c.Movies]);
            return View(cinemas);
        }

        public async Task<IActionResult> Actors()
        {
            var actors = await _actorRepository.GetAllAsync(includes: [a => a.MovieActors]);
            return View(actors);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}