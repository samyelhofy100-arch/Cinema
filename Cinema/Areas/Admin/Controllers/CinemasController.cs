
using Cinema.Data;
using Cinema.Helpers;
using Cinema.Models;
using Cinema.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRoles)]
    public class CinemasController : Controller
    {
        private readonly IRepository<Cinemas> _cinemasRepository;
        private readonly IRepository<Hall> _hallsRepository;

        private readonly string Folder =
            Path.Combine("Images", "CinemaImg");

        public CinemasController(
            IRepository<Cinemas> cinemasRepository,
            IRepository<Hall> hallsRepository)
        {
            _cinemasRepository = cinemasRepository;
            _hallsRepository = hallsRepository;
        }

        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemasRepository.GetAllAsync(
                includes: [c => c.Movies],
                orderBy: p => p.OrderBy(c => c.Name));

            return View(cinemas);
        }

        public async Task<IActionResult> Details(int id)
        {
            var cinema = await _cinemasRepository.GetOneAsync(
                c => c.Id == id,
                includes: [c => c.Movies]);

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
        public async Task<IActionResult> Create(
            Cinemas cinema,
            IFormFile? ImageFile)
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

            await _cinemasRepository.AddAsync(cinema);
            await _cinemasRepository.CommitAsync();

            TempData["Success"] =
                $"Cinema \"{cinema.Name}\" was created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _cinemasRepository.FindAsync(id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            Cinemas input,
            IFormFile? ImageFile)
        {
            var cinema = await _cinemasRepository.FindAsync(input.Id);

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

            await _cinemasRepository.CommitAsync();

            TempData["Success"] =
                $"Cinema \"{cinema.Name}\" was updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _cinemasRepository.GetOneAsync(
                c => c.Id == id,
                includes: [c => c.Movies]);

            if (cinema == null)
                return NotFound();

            // Check if cinema has movies.
            if (cinema.Movies.Any())
            {
                TempData["Error"] =
                    "Cannot delete a cinema that has movies assigned to it.";

                return RedirectToAction(nameof(Index));
            }

            // Check if cinema has halls.
            var hasHalls = await _hallsRepository
                .GetOneAsync(h => h.CinemaId == id);

            if (hasHalls != null)
            {
                TempData["Error"] =
                    $"\"{cinema.Name}\" still has halls. Delete them first.";

                return RedirectToAction(nameof(Index));
            }

            DeleteImage(cinema.Img);

            _cinemasRepository.Remove(cinema);

            await _cinemasRepository.CommitAsync();

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
