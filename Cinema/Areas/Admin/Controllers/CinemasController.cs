
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemasController : Controller
    {
        private readonly ApplicationDbContext _context =
            new ApplicationDbContext();

        private readonly string Folder =
            Path.Combine("Images", "CinemaImg");

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
                var extension =
                    Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                var allowedExtensions = new[]
                {
                    ".jpg", ".jpeg", ".png", ".webp"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "Img",
                        "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                }
                else if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "Img",
                        "Image size must not exceed 5 MB.");
                }
                else
                {
                    cinema.Img = SaveImage(ImageFile);
                }
            }

            if (!ModelState.IsValid)
                return View(cinema);

            _context.Cinemas.Add(cinema);
            _context.SaveChanges();

            TempData["Success"] =
                $"Cinema \"{cinema.Name}\" was created successfully.";

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
                var extension =
                    Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                var allowedExtensions = new[]
                {
                    ".jpg", ".jpeg", ".png", ".webp"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "Img",
                        "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                }
                else if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "Img",
                        "Image size must not exceed 5 MB.");
                }

                if (!ModelState.IsValid)
                    return View(input);

                var newImage = SaveImage(ImageFile);

                DeleteImage(cinema.Img);

                cinema.Img = newImage;
            }

            if (!ModelState.IsValid)
                return View(input);

            cinema.Name = input.Name;
            cinema.Description = input.Description;

            _context.SaveChanges();

            TempData["Success"] =
                $"Cinema \"{cinema.Name}\" was updated successfully.";

            return RedirectToAction(nameof(Index));
        }

       
        public IActionResult Delete(int id)
        {
            var cinema = _context.Cinemas
                .Include(c => c.Movies)
                .FirstOrDefault(c => c.Id == id);

            if (cinema == null)
                return NotFound();

            if (cinema.Movies.Any())
            {
                TempData["Error"] =
                    "Cannot delete a cinema that has movies assigned to it.";

                return RedirectToAction(nameof(Index));
            }

            DeleteImage(cinema.Img);

            _context.Cinemas.Remove(cinema);
            _context.SaveChanges();

            TempData["Success"] =
                $"Cinema \"{cinema.Name}\" was deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private string SaveImage(IFormFile imageFile)
        {
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                Folder);

            Directory.CreateDirectory(folderPath);

            var extension =
                Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            var fileName = $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(
                filePath,
                FileMode.Create);

            imageFile.CopyTo(stream);

            return fileName;
        }

        private void DeleteImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var safeFileName = Path.GetFileName(fileName);

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                Folder,
                safeFileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }
}
