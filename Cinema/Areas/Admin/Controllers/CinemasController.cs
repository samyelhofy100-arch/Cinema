using Cinema.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemasController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public IActionResult Index()
        {
            var cinemas = _context.Cinemas
                .Include(c => c.Movies)
                .OrderBy(c => c.Name)
                .ToList();

            return View(cinemas);
        }
        public IActionResult Details(int id)
        {
            var cinema = _context.Cinemas
                .Include(c => c.Movies)
                .FirstOrDefault(c => c.Id == id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Cinemas());
        }

        [HttpPost]
        public IActionResult Create(Cinemas cinema, IFormFile? ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!ImageHelper.IsValid(ImageFile, out var error))
                {
                    ModelState.AddModelError("Img", error);
                }
                else
                {
                    cinema.Img = ImageHelper.Save(ImageFile, "wwwroot/Images", "CinemaImg");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(cinema);
            }

            _context.Cinemas.Add(cinema);
            _context.SaveChanges();

            TempData["Success"] = $"Cinema \"{cinema.Name}\" was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cinema = _context.Cinemas.Find(id);
            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpPost]
        public IActionResult Edit(Cinemas input, IFormFile? ImageFile)
        {
            var cinema = _context.Cinemas.Find(input.Id);
            if (cinema == null)
                return NotFound();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!ImageHelper.IsValid(ImageFile, out var error))
                {
                    ModelState.AddModelError("Img", error);
                    return View(input);
                }

                if (!string.IsNullOrEmpty(cinema.Img))
                {
                    ImageHelper.Delete("wwwroot/Images", "CinemaImg", cinema.Img);
                }

                cinema.Img = ImageHelper.Save(ImageFile, "wwwroot/Images", "CinemaImg");

                
            }

            cinema.Name = input.Name;
            cinema.Description = input.Description;

            _context.SaveChanges();

            TempData["Success"] = $"Cinema \"{cinema.Name}\" was updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var cinema = _context.Cinemas
                .Include(c => c.Movies)
                .FirstOrDefault(c => c.Id == id);

            if (cinema == null)
                return NotFound();

            if (!string.IsNullOrEmpty(cinema.Img))
            {
                ImageHelper.Delete("wwwroot/Images", "CinemaImg", cinema.Img);
            }
            _context.Cinemas.Remove(cinema);
            _context.SaveChanges();

            TempData["Success"] = $"Cinema \"{cinema.Name}\" was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        }
}
