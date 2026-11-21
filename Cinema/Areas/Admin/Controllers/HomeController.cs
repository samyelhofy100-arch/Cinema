
using Cinema.Data;
using Cinema.Helpers;
using Cinema.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRoles)]
    public class HomeController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Cinemas> _cinemaRepository;
        private readonly IRepository<Order> _orderRepository;

        public HomeController(
            IRepository<Movie> movieRepository,
            IRepository<Actor> actorRepository,
            IRepository<Category> categoryRepository,
            IRepository<Cinemas> cinemaRepository,
            IRepository<Order> orderRepository)
        {
            _movieRepository = movieRepository;
            _actorRepository = actorRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.MoviesCount =
                await _movieRepository.CountAsync();

            ViewBag.AvailableMovies =
                await _movieRepository.CountAsync(m => m.Status);

            ViewBag.ActorsCount =
                await _actorRepository.CountAsync();

            ViewBag.CinemasCount =
                await _cinemaRepository.CountAsync();

            ViewBag.CategoriesCount =
                await _categoryRepository.CountAsync();

            // Count paid orders only
            ViewBag.OrdersCount =
                await _orderRepository.CountAsync(
                    o => o.Status == OrderStatus.Paid
                );

            var latestMovies = await _movieRepository.GetAllAsync(
                includes: [m => m.Category, m => m.Cinema],
                orderBy: q => q.OrderByDescending(m => m.DateTime),
                take: 5
            );

            return View(latestMovies);
        }
    }
}
