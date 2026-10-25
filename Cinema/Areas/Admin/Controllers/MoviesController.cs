using Cinema.Data;
using Cinema.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext context = new ApplicationDbContext();

        private const string Folder = "movies";

        public IActionResult Index()
        {
            var movies = context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .OrderBy(m => m.Name)
                .ToList();

            return View(movies);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadData();
            return View(new Movie());
        }

        [HttpPost]
        public IActionResult Create(
            Movie movie,
            IFormFile? ImageFile,
            List<IFormFile>? SubImages,
            List<int>? ActorIds)
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError("MainImg", "Please select a main image.");
            }
            else if (!ImageHelper.IsValid(ImageFile, out var error))
            {
                ModelState.AddModelError("MainImg", error);
            }

            if (SubImages != null)
            {
                foreach (var image in SubImages.Where(x => x != null && x.Length > 0))
                {
                    if (!ImageHelper.IsValid(image, out var subError))
                    {
                        ModelState.AddModelError("SubImages", subError);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                LoadData();
                return View(movie);
            }

            movie.MainImg = ImageHelper.Save(ImageFile!, "wwwroot", Folder);

            context.Movies.Add(movie);
            context.SaveChanges();

            if (SubImages != null)
            {
                foreach (var image in SubImages.Where(x => x != null && x.Length > 0))
                {
                    var fileName = ImageHelper.Save(image, "wwwroot", Folder);

                    context.MovieSubImages.Add(new MovieSubImage
                    {
                        Img = fileName,
                        MovieId = movie.Id
                    });
                }
            }

            if (ActorIds != null)
            {
                foreach (var actorId in ActorIds.Distinct())
                {
                    if (context.Actors.Any(a => a.Id == actorId))
                    {
                        context.MovieActors.Add(new MovieActor
                        {
                            MovieId = movie.Id,
                            ActorId = actorId
                        });
                    }
                }
            }

            context.SaveChanges();

            TempData["Success"] = $"Movie \"{movie.Name}\" was created.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = context.Movies
                .Include(m => m.MovieActors)
                .Include(m => m.SubImage)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            LoadData();
            return View(movie);
        }

        [HttpPost]
        public IActionResult Edit(
            Movie input,
            IFormFile? ImageFile,
            List<IFormFile>? SubImages,
            List<int>? ActorIds)
        {
            var movie = context.Movies
                .Include(m => m.MovieActors)
                .Include(m => m.SubImage)
                .FirstOrDefault(m => m.Id == input.Id);

            if (movie == null)
                return NotFound();

            if (ImageFile is { Length: > 0 } &&
                !ImageHelper.IsValid(ImageFile, out var error))
            {
                ModelState.AddModelError("ImageFile", error);
            }

            if (SubImages != null)
            {
                foreach (var image in SubImages.Where(x => x != null && x.Length > 0))
                {
                    if (!ImageHelper.IsValid(image, out var subError))
                    {
                        ModelState.AddModelError("SubImages", subError);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                LoadData();
                return View(movie);
            }

            movie.Name = input.Name;
            movie.Description = input.Description;
            movie.Price = input.Price;
            movie.Status = input.Status;
            movie.DateTime = input.DateTime;
            movie.CategoryId = input.CategoryId;
            movie.CinemaId = input.CinemaId;

            if (ImageFile is { Length: > 0 })
            {
                var oldImage = movie.MainImg;

                movie.MainImg = ImageHelper.Save(ImageFile, "wwwroot", Folder);

                ImageHelper.Delete("wwwroot", Folder, oldImage);
            }

            if (SubImages != null &&
                SubImages.Any(x => x != null && x.Length > 0))
            {
                foreach (var oldSubImage in movie.SubImage.ToList())
                {
                    ImageHelper.Delete("wwwroot", Folder, oldSubImage.Img);
                }

                context.MovieSubImages.RemoveRange(movie.SubImage);

                foreach (var image in SubImages.Where(x => x != null && x.Length > 0))
                {
                    var fileName = ImageHelper.Save(image, "wwwroot", Folder);

                    context.MovieSubImages.Add(new MovieSubImage
                    {
                        Img = fileName,
                        MovieId = movie.Id
                    });
                }
            }

            context.MovieActors.RemoveRange(movie.MovieActors);

            if (ActorIds != null)
            {
                foreach (var actorId in ActorIds.Distinct())
                {
                    if (context.Actors.Any(a => a.Id == actorId))
                    {
                        context.MovieActors.Add(new MovieActor
                        {
                            MovieId = movie.Id,
                            ActorId = actorId
                        });
                    }
                }
            }

            context.SaveChanges();

            TempData["Success"] = $"Movie \"{movie.Name}\" was updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var movie = context.Movies
                .Include(m => m.MovieActors)
                .Include(m => m.SubImage)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            var movieName = movie.Name;

            ImageHelper.Delete("wwwroot", Folder, movie.MainImg);

            foreach (var image in movie.SubImage)
            {
                ImageHelper.Delete("wwwroot", Folder, image.Img);
            }

            context.MovieActors.RemoveRange(movie.MovieActors);
            context.MovieSubImages.RemoveRange(movie.SubImage);
            context.Movies.Remove(movie);

            context.SaveChanges();

            TempData["Success"] = $"Movie \"{movieName}\" was deleted.";

            return RedirectToAction(nameof(Index));
        }

        private void LoadData()
        {
            ViewBag.Categories = context.Categories
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Cinemas = context.Cinemas
                .ToList();

            ViewBag.Actors = context.Actors
                .OrderBy(a => a.Name)
                .ToList();
        }
    }
}