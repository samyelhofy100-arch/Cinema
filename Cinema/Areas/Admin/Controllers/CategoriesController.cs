using Cinema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext context = new ApplicationDbContext();

        public IActionResult Index()
        {
            var categories = context.Categories
                .Include(c => c.Movies)
                .OrderBy(c => c.Name)
                .ToList();

            return View(categories);
        }

        public IActionResult Details(int id)
        {
            var category = context.Categories
                .Include(c => c.Movies)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            category.Name = category.Name.Trim();
            context.Categories.Add(category);
            context.SaveChanges();

            TempData["Success"] = $"Category \"{category.Name}\" was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = context.Categories.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category input)
        {
            var category = context.Categories.Find(input.Id);
            if (category == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            category.Name = input.Name.Trim();
            context.SaveChanges();

            TempData["Success"] = $"Category \"{category.Name}\" was updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var category = context.Categories
                .Include(c => c.Movies)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                return NotFound();

            var categoryName = category.Name;

            context.Categories.Remove(category);
            context.SaveChanges();

            TempData["Success"] = $"Category \"{categoryName}\" was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}