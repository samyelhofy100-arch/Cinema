
using Cinema.Helpers;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext context =
            new ApplicationDbContext();

        private const string Folder = "Images/ActorImg";

        public IActionResult Index(string? search)
        {
            var actors = context.Actors
                .Include(a => a.MovieActors)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                actors = actors.Where(a =>
                    a.Name.Contains(search));
            }

            ViewBag.Search = search;

            return View(actors.OrderBy(a => a.Name).ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Actor());
        }

        [HttpPost]
        public IActionResult Create(Actor actor, IFormFile? imgFile)
        {
            if (imgFile is { Length: > 0 } &&
                !ImageHelper.IsValid(imgFile, out var error))
            {
                ModelState.AddModelError("imgFile", error);
            }

            if (!ModelState.IsValid)
            {
                return View("Form", actor);
            }

            actor.Name = actor.Name.Trim();

            if (imgFile is { Length: > 0 })
            {
                actor.Img = ImageHelper.Save(
                    imgFile, "wwwroot", Folder);
            }

            context.Actors.Add(actor);
            context.SaveChanges();

            TempData["Success"] =
                $"Actor \"{actor.Name}\" was created.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var actor = context.Actors.Find(id);

            if (actor == null)
            {
                return NotFound();
            }

            return View("Edit", actor);
        }

        [HttpPost]
        public IActionResult Edit(Actor input, IFormFile? imgFile)
        {
            var actor = context.Actors.Find(input.Id);

            if (actor == null)
            {
                return NotFound();
            }

            if (imgFile is { Length: > 0 } &&
                !ImageHelper.IsValid(imgFile, out var error))
            {
                ModelState.AddModelError("imgFile", error);
            }

            if (!ModelState.IsValid)
            {
                input.Img = actor.Img;
                return View("Form", input);
            }

            actor.Name = input.Name.Trim();

            if (imgFile is { Length: > 0 })
            {
                var oldImage = actor.Img;

                actor.Img = ImageHelper.Save(
                    imgFile, "wwwroot", Folder);

                ImageHelper.Delete(
                    "wwwroot", Folder, oldImage);
            }

            context.SaveChanges();

            TempData["Success"] =
                $"Actor \"{actor.Name}\" was updated.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var actor = context.Actors
                .Include(a => a.MovieActors)
                .FirstOrDefault(a => a.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            var actorName = actor.Name;

            ImageHelper.Delete(
                "wwwroot", Folder, actor.Img);

            context.MovieActors.RemoveRange(actor.MovieActors);
            context.Actors.Remove(actor);

            context.SaveChanges();

            TempData["Success"] =
                $"Actor \"{actorName}\" was deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}
