using Cinema.Areas.Admin.ViewModels;
namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRoles)]
    public class HallsController : Controller
    {
        private readonly ApplicationDbContext context;

        public HallsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var halls = context.Halls
                .Include(h => h.Cinema)
                .Include(h => h.Showtimes)
                .OrderBy(h => h.Cinema.Name).ThenBy(h => h.Name)
                .ToList();

            return View(halls);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new HallFormVM();
            FillCinemas(vm);
            return View("Form", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HallFormVM vm)
        {
            vm.Name = (vm.Name ?? string.Empty).Trim();

            if (ModelState.IsValid && context.Halls.Any(h => h.CinemaId == vm.CinemaId && h.Name == vm.Name))
                ModelState.AddModelError(nameof(vm.Name), "This cinema already has a hall with this name.");

            if (ModelState.IsValid && !context.Cinemas.Any(c => c.Id == vm.CinemaId))
                ModelState.AddModelError(nameof(vm.CinemaId), "Please select a cinema.");

            if (!ModelState.IsValid)
            {
                FillCinemas(vm);
                return View("Form", vm);
            }

            var hall = new Hall
            {
                Name = vm.Name,
                CinemaId = vm.CinemaId,
                Rows = vm.Rows,
                SeatsPerRow = vm.SeatsPerRow,
                Seats = SeatLayout.Generate(vm.Rows, vm.SeatsPerRow)
            };

            context.Halls.Add(hall);
            await context.SaveChangesAsync();

            TempData["Success"] = $"Hall \"{hall.Name}\" was added with {hall.Seats.Count} seats.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var hall = context.Halls.Include(h => h.Cinema).FirstOrDefault(h => h.Id == id);
            if (hall == null) return NotFound();

            return View("Form", new HallFormVM
            {
                Id = hall.Id,
                Name = hall.Name,
                CinemaId = hall.CinemaId,
                Rows = hall.Rows,
                SeatsPerRow = hall.SeatsPerRow,
                CinemaName = hall.Cinema.Name
            });
        }

   
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HallFormVM vm)
        {
            var hall = context.Halls.Include(h => h.Cinema).FirstOrDefault(h => h.Id == vm.Id);
            if (hall == null) return NotFound();

            vm.Name = vm.Name.Trim();

            if (ModelState.IsValid &&
                context.Halls.Any(h => h.Id != hall.Id && h.CinemaId == hall.CinemaId && h.Name == vm.Name))
                ModelState.AddModelError(nameof(vm.Name), "This cinema already has a hall with this name.");

            if (!ModelState.IsValid)
            {
                vm.CinemaId = hall.CinemaId;
                vm.Rows = hall.Rows;
                vm.SeatsPerRow = hall.SeatsPerRow;
                vm.CinemaName = hall.Cinema.Name;
                return View("Form", vm);
            }

            hall.Name = vm.Name;
            await context.SaveChangesAsync();

            TempData["Success"] = $"Hall \"{hall.Name}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hall = context.Halls.Include(h => h.Showtimes).FirstOrDefault(h => h.Id == id);
            if (hall == null) return NotFound();

            if (hall.Showtimes.Any())
            {
                TempData["Error"] = $"\"{hall.Name}\" still has {hall.Showtimes.Count} showtime(s). Delete them first.";
                return RedirectToAction(nameof(Index));
            }

            context.Halls.Remove(hall);   
            await context.SaveChangesAsync();

            TempData["Success"] = $"Hall \"{hall.Name}\" was deleted.";
            return RedirectToAction(nameof(Index));
        }

        private void FillCinemas(HallFormVM vm)
        {
            vm.Cinemas = context.Cinemas.OrderBy(c => c.Name).ToList();
        }
    }
}