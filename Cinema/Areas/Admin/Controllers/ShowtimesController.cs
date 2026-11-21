using Cinema.Areas.Admin.ViewModels;
using System.Globalization;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRoles)]
    public class ShowtimesController : Controller
    {
        // موعدين في نفس القاعة لازم يبعدوا عن بعض بالدقايق دي على الأقل
        private const int SlotMinutes = 180;

        private readonly ApplicationDbContext context;

        public ShowtimesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // ---------- القايمة ----------
        public IActionResult Index(int? movieId, DateTime? date, bool past = false)
        {
            var now = DateTime.Now;

            var query = context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall).ThenInclude(h => h.Cinema)
                .AsQueryable();

            if (movieId.HasValue)
                query = query.Where(s => s.MovieId == movieId);

            if (date.HasValue)
            {
                var day = date.Value.Date;
                var nextDay = day.AddDays(1);
                query = query.Where(s => s.StartTime >= day && s.StartTime < nextDay);
            }

            var ordered = past
                ? query.Where(s => s.StartTime <= now).OrderByDescending(s => s.StartTime)
                : query.Where(s => s.StartTime > now).OrderBy(s => s.StartTime);

            ViewBag.Movies = context.Movies.OrderBy(m => m.Name).ToList();
            ViewBag.MovieId = movieId;
            ViewBag.Date = date;
            ViewBag.Past = past;

            return View(ordered.Take(300).ToList());
        }

        // ---------- إضافة ----------
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new ShowtimeCreateVM { StartDate = DateTime.Today.AddDays(1) };
            Fill(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShowtimeCreateVM vm)
        {
            var times = ParseTimes(vm.Times ?? string.Empty, out var badToken);
            if (badToken != null)
                ModelState.AddModelError(nameof(vm.Times), $"\"{badToken}\" is not a valid time. Use 24-hour format like 14:00.");
            else if (times.Count == 0)
                ModelState.AddModelError(nameof(vm.Times), "Enter at least one time, e.g. 14:00, 17:30");

            if (ModelState.IsValid && !context.Movies.Any(m => m.Id == vm.MovieId))
                ModelState.AddModelError(nameof(vm.MovieId), "Please select a movie.");

            if (ModelState.IsValid && !context.Halls.Any(h => h.Id == vm.HallId))
                ModelState.AddModelError(nameof(vm.HallId), "Please select a hall.");

            if (!ModelState.IsValid)
            {
                Fill(vm);
                return View(vm);
            }

            var now = DateTime.Now;
            var added = new List<Showtime>();
            int skippedPast = 0, skippedOverlap = 0;

            for (int d = 0; d < vm.Days; d++)
            {
                foreach (var time in times)
                {
                    var start = vm.StartDate!.Value.Date.AddDays(d).Add(time);

                    if (start <= now) { skippedPast++; continue; }

                    bool overlapsExisting = HasConflict(vm.HallId, start, null);
                    bool overlapsNew = added.Any(a => Math.Abs((a.StartTime - start).TotalMinutes) < SlotMinutes);
                    if (overlapsExisting || overlapsNew) { skippedOverlap++; continue; }

                    added.Add(new Showtime
                    {
                        MovieId = vm.MovieId,
                        HallId = vm.HallId,
                        StartTime = start,
                        Price = vm.Price
                    });
                }
            }

            if (added.Count == 0)
            {
                ModelState.AddModelError(string.Empty,
                    $"Nothing was added ({skippedPast} in the past, {skippedOverlap} overlapping another showtime in this hall).");
                Fill(vm);
                return View(vm);
            }

            context.Showtimes.AddRange(added);
            await context.SaveChangesAsync();

            var message = $"{added.Count} showtime(s) added.";
            if (skippedPast + skippedOverlap > 0)
                message += $" Skipped {skippedPast} in the past and {skippedOverlap} overlapping.";

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ---------- تعديل (الوقت والسعر بس) ----------
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var showtime = Load(id);
            if (showtime == null) return NotFound();

            if (showtime.StartTime <= DateTime.Now)
            {
                TempData["Error"] = "Past showtimes cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(new ShowtimeEditVM
            {
                Id = showtime.Id,
                StartTime = showtime.StartTime,
                Price = showtime.Price,
                MovieName = showtime.Movie.Name,
                HallName = $"{showtime.Hall.Cinema.Name} - {showtime.Hall.Name}"
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShowtimeEditVM vm)
        {
            var showtime = Load(vm.Id);
            if (showtime == null) return NotFound();

            vm.MovieName = showtime.Movie.Name;
            vm.HallName = $"{showtime.Hall.Cinema.Name} - {showtime.Hall.Name}";

            if (ModelState.IsValid)
            {
                var start = vm.StartTime!.Value;

                if (start <= DateTime.Now)
                    ModelState.AddModelError(nameof(vm.StartTime), "The start time must be in the future.");
                else if (HasConflict(showtime.HallId, start, showtime.Id))
                    ModelState.AddModelError(nameof(vm.StartTime), "This overlaps another showtime in the same hall.");
            }

            if (!ModelState.IsValid) return View(vm);

            showtime.StartTime = vm.StartTime!.Value;
            showtime.Price = vm.Price;
            await context.SaveChangesAsync();

            TempData["Success"] = "Showtime updated.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- حذف ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var showtime = await context.Showtimes.FindAsync(id);
            if (showtime == null) return NotFound();

            try
            {
                context.Showtimes.Remove(showtime);
                await context.SaveChangesAsync();
                TempData["Success"] = "Showtime deleted.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "This showtime has bookings and cannot be deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------
        private Showtime? Load(int id) =>
            context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall).ThenInclude(h => h.Cinema)
                .FirstOrDefault(s => s.Id == id);

        private bool HasConflict(int hallId, DateTime start, int? ignoreId)
        {
            var from = start.AddMinutes(-SlotMinutes);
            var to = start.AddMinutes(SlotMinutes);
            int ignore = ignoreId ?? 0;

            return context.Showtimes.Any(s =>
                s.HallId == hallId && s.Id != ignore && s.StartTime > from && s.StartTime < to);
        }

        private void Fill(ShowtimeCreateVM vm)
        {
            vm.Movies = context.Movies.OrderBy(m => m.Name).ToList();
            vm.Halls = context.Halls.Include(h => h.Cinema)
                .OrderBy(h => h.Cinema.Name).ThenBy(h => h.Name).ToList();
        }

        private static List<TimeSpan> ParseTimes(string input, out string? bad)
        {
            bad = null;
            var result = new List<TimeSpan>();
            var tokens = input.Split(new[] { ',', ';', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                if (TimeSpan.TryParseExact(token, new[] { @"h\:mm", @"hh\:mm" }, CultureInfo.InvariantCulture, out var ts))
                {
                    if (!result.Contains(ts)) result.Add(ts);
                }
                else
                {
                    bad = token;
                    return result;
                }
            }

            return result.OrderBy(t => t).ToList();
        }
    }
}