using Cinema.Data;
using Cinema.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
       // private readonly ApplicationDbContext _context;//= new ApplicationDbContext();
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Cinemas> _cinemaRepository;

        public HomeController(IRepository<Movie> movieRepository, IRepository<Actor> actorRepository, IRepository<Category> categoryRepository, IRepository<Cinemas> cinemaRepository)
        {
            _movieRepository = movieRepository;
            _actorRepository = actorRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
        }

        public async Task<IActionResult> Index()
        {
            //ViewBag.MoviesCount = _context.Movies.Count();
            ViewBag.MoviesCount =await _movieRepository.CountAsync();

            //ViewBag.AvailableMovies = _context.Movies.Count(m => m.Status);
            ViewBag.AvailableMovies = await _movieRepository.CountAsync(m => m.Status);

            // ViewBag.ActorsCount = _context.Actors.Count();
            ViewBag.ActorsCount = await _actorRepository.CountAsync();

            // ViewBag.CinemasCount = _context.Cinemas.Count();
            ViewBag.CinemasCount = await _cinemaRepository.CountAsync();

            // ViewBag.CategoriesCount = _context.Categories.Count();
            ViewBag.CategoriesCount = await _categoryRepository.CountAsync();

            //var latestMovies = _context.Movies
            //    .Include(m => m.Category)
            //    .Include(m => m.Cinema)
            //    .OrderByDescending(m => m.DateTime)
            //    .Take(5)
            //    .ToList();
            var latestMovies =await _movieRepository.GetAllAsync(includes: [m => m.Category , m => m.Cinema], orderBy: q => q.OrderByDescending(m => m.DateTime), take: 5 );

            return View(latestMovies);
        }
    }
}