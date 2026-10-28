
using Cinema.Data;
using Cinema.Models;
using Cinema.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorsController : Controller
    {
       // private readonly ApplicationDbContext context =
           // new ApplicationDbContext();

        private readonly IRepository<Actor> _actorRepository;
        private readonly IMovieActorRepository _movieActorRepository;
        private readonly string Folder =
            Path.Combine("wwwroot", "Images", "ActorImg");

        public ActorsController(IRepository<Actor> actorRepository, IMovieActorRepository movieActorRepository)
        {
            _actorRepository = actorRepository;
            _movieActorRepository = movieActorRepository;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 5;

            // var actors = context.Actors.AsQueryable();
            var actors = await _actorRepository.GetAllAsync(includes: [a => a.MovieActors]);
            if (!string.IsNullOrWhiteSpace(search))
            {
                actors = actors.Where(a => a.Name.Contains(search));
            }

            int totalActors = actors.Count();

            int totalPages = (int)Math.Ceiling(totalActors / (double)pageSize);

            if (totalPages > 0)
            {
                page = Math.Clamp(page, 1, totalPages);
            }
            else
            {
                page = 1;
            }

            var result = actors
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(result);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View(new Actor());
        }

        [HttpPost]
        
        public async Task<IActionResult> Create(Actor actor, IFormFile? imgFile)
        {
            if (imgFile != null && imgFile.Length > 0)
            {
                if (!IsValidImage(imgFile))
                {
                    ModelState.AddModelError(
                        "imgFile",
                        "Please upload a valid JPG, PNG, or WEBP image.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(actor);
            }

            actor.Name = actor.Name.Trim();

            if (imgFile != null && imgFile.Length > 0)
            {
                actor.Img = SaveImage(imgFile);
            }

           // context.Actors.Add(actor);
          await _actorRepository.AddAsync(actor);
          //  context.SaveChanges();
          await _actorRepository.CommitAsync();
            TempData["Success"] =
                $"Actor \"{actor.Name}\" was created.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var actor = await _actorRepository.GetOneAsync(
                a => a.Id == id,
                includeProperties: ["MovieActors.Movie"],
                isTracked: false);

            if (actor == null)
                return NotFound();

            return View(actor);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
           // var actor = context.Actors.Find(id);
           var actor =await _actorRepository.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        [HttpPost]
        
        public async Task<IActionResult> Edit(Actor input, IFormFile? imgFile)
        {
           // var actor = context.Actors.Find(input.Id);
           var actor =await _actorRepository.FindAsync(input.Id);   
            if (actor == null)
            {
                return NotFound();
            }

            if (imgFile != null && imgFile.Length > 0)
            {
                if (!IsValidImage(imgFile))
                {
                    ModelState.AddModelError(
                        "imgFile",
                        "Please upload a valid JPG, PNG, or WEBP image.");
                }
            }

            if (!ModelState.IsValid)
            {
                input.Img = actor.Img;
                return View(input);
            }

            actor.Name = input.Name.Trim();

            if (imgFile != null && imgFile.Length > 0)
            {
                var oldImage = actor.Img;
                actor.Img = SaveImage(imgFile);

              //  context.SaveChanges();
             await _actorRepository.CommitAsync();
                DeleteImage(oldImage);
            }

            // context.SaveChanges();
            await _actorRepository.CommitAsync();

            TempData["Success"] =
                $"Actor \"{actor.Name}\" was updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        
        public async Task<IActionResult> Delete(int id)
        {
            //var actor = context.Actors
            //    .Include(a => a.MovieActors)
            //    .FirstOrDefault(a => a.Id == id);

            var actor =await _actorRepository.GetOneAsync(a => a.Id == id,includes: [a => a.MovieActors]);
            if (actor == null)
            {
                return NotFound();
            }

            var actorName = actor.Name;
            var oldImage = actor.Img;
            //********
            // context.MovieActors.RemoveRange(actor.MovieActors);

            _movieActorRepository.RemoveRange(actor.MovieActors);
            //_actorRepository.Remove(actor);
            _actorRepository.Remove(actor);

           // context.SaveChanges();
           await _actorRepository.CommitAsync();

            DeleteImage(oldImage);

            TempData["Success"] =
                $"Actor \"{actorName}\" was deleted.";

            return RedirectToAction(nameof(Index));
        }

        private string SaveImage(IFormFile image)
        {
            var webRoot = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot");

            var folderPath = Path.Combine(
                webRoot, "Images", "ActorImg");

            Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(image.FileName)
                .ToLowerInvariant();

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }

            return fileName;
        }

        private void DeleteImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var webRoot = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot");

            var filePath = Path.Combine(
                webRoot, "Images", "ActorImg", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        private bool IsValidImage(IFormFile image)
        {
            var extension = Path.GetExtension(image.FileName)
                .ToLowerInvariant();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            return image.Length > 0
                && allowedExtensions.Contains(extension);
        }
    }
}
