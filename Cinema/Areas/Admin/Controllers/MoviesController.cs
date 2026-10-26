
namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext context = new ApplicationDbContext();

        private const string Folder = "Images/MoiveImg";
        private const long MaxImageSize = 5 * 1024 * 1024;

        private readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        public IActionResult Index(int page = 1)
        {
            int pageSize = 8;
            int totalMovies = context.Movies.Count();

            int totalPages = (int)Math.Ceiling(
                totalMovies / (double)pageSize
            );

            page = totalPages == 0
                ? 1
                : Math.Clamp(page, 1, totalPages);

            var movies = context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .OrderBy(m => m.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

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
                ModelState.AddModelError(
                    "ImageFile",
                    "Please select a main image."
                );
            }
            else if (!IsValidImage(ImageFile, out string error))
            {
                ModelState.AddModelError("ImageFile", error);
            }

            if (SubImages != null)
            {
                foreach (var image in SubImages.Where(
                    x => x != null && x.Length > 0))
                {
                    if (!IsValidImage(image, out string error))
                    {
                        ModelState.AddModelError("SubImages", error);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e =>
                        $"{x.Key}: {e.ErrorMessage}"));

                TempData["CreateErrors"] = string.Join(" | ", errors);

                LoadData();
                return View(movie);
            }

            movie.MainImg = SaveImage(ImageFile!);

            context.Movies.Add(movie);
            context.SaveChanges();

            if (SubImages != null)
            {
                foreach (var image in SubImages.Where(
                    x => x != null && x.Length > 0))
                {
                    string fileName = SaveImage(image);

                    context.MovieSubImages.Add(
                        new MovieSubImage
                        {
                            Img = fileName,
                            MovieId = movie.Id
                        }
                    );
                }
            }

            if (ActorIds != null)
            {
                foreach (int actorId in ActorIds.Distinct())
                {
                    if (context.Actors.Any(a => a.Id == actorId))
                    {
                        context.MovieActors.Add(
                            new MovieActor
                            {
                                MovieId = movie.Id,
                                ActorId = actorId
                            }
                        );
                    }
                }
            }

            context.SaveChanges();

            TempData["Success"] =
                $"Movie \"{movie.Name}\" was created.";

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

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!IsValidImage(ImageFile, out string error))
                {
                    ModelState.AddModelError("ImageFile", error);
                }
            }

            if (SubImages != null)
            {
                foreach (var image in SubImages.Where(
                    x => x != null && x.Length > 0))
                {
                    if (!IsValidImage(image, out string error))
                    {
                        ModelState.AddModelError("SubImages", error);
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

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string oldImage = movie.MainImg;
                movie.MainImg = SaveImage(ImageFile);

                DeleteImage(oldImage);
            }

            if (SubImages != null &&
                SubImages.Any(x => x != null && x.Length > 0))
            {
                foreach (var oldSubImage in movie.SubImage.ToList())
                {
                    DeleteImage(oldSubImage.Img);
                }

                context.MovieSubImages.RemoveRange(movie.SubImage);

                foreach (var image in SubImages.Where(
                    x => x != null && x.Length > 0))
                {
                    string fileName = SaveImage(image);

                    context.MovieSubImages.Add(
                        new MovieSubImage
                        {
                            Img = fileName,
                            MovieId = movie.Id
                        }
                    );
                }
            }

            context.MovieActors.RemoveRange(movie.MovieActors);

            if (ActorIds != null)
            {
                foreach (int actorId in ActorIds.Distinct())
                {
                    if (context.Actors.Any(a => a.Id == actorId))
                    {
                        context.MovieActors.Add(
                            new MovieActor
                            {
                                MovieId = movie.Id,
                                ActorId = actorId
                            }
                        );
                    }
                }
            }

            context.SaveChanges();

            TempData["Success"] =
                $"Movie \"{movie.Name}\" was updated.";

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

            string movieName = movie.Name;

            DeleteImage(movie.MainImg);

            foreach (var image in movie.SubImage)
            {
                DeleteImage(image.Img);
            }

            context.MovieActors.RemoveRange(movie.MovieActors);
            context.MovieSubImages.RemoveRange(movie.SubImage);
            context.Movies.Remove(movie);

            context.SaveChanges();

            TempData["Success"] =
                $"Movie \"{movieName}\" was deleted.";

            return RedirectToAction(nameof(Index));
        }

        private bool IsValidImage(
            IFormFile file,
            out string error)
        {
            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                error = "Only JPG, JPEG, PNG and WEBP images are allowed.";
                return false;
            }

            if (file.Length > MaxImageSize)
            {
                error = "Image size must not exceed 5 MB.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private string SaveImage(IFormFile file)
        {
            string uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                Folder
            );

            Directory.CreateDirectory(uploadsFolder);

            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            string fileName = $"{Guid.NewGuid()}{extension}";

            string filePath = Path.Combine(
                uploadsFolder,
                fileName
            );

            using var stream = new FileStream(
                filePath,
                FileMode.Create
            );

            file.CopyTo(stream);

            return fileName;
        }

        private void DeleteImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            // Only allow deleting files inside the movies folder.
            if (Path.GetFileName(fileName) != fileName)
                return;

            string filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                Folder,
                fileName
            );

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        private void LoadData()
        {
            ViewBag.Categories = context.Categories
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Cinemas = context.Cinemas.ToList();

            ViewBag.Actors = context.Actors
                .OrderBy(a => a.Name)
                .ToList();
        }
    }
}
