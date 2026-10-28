using Cinema.Data;
using Cinema.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        // private readonly ApplicationDbContext context = new ApplicationDbContext();

        private readonly IRepository<Category> _categoryRepository;

        public CategoriesController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            //var categories = context.Categories
            //    .Include(c => c.Movies)
            //    .OrderBy(c => c.Name)
            //    .ToList();
            var categories = await _categoryRepository.GetAllAsync(includes: [c => c.Movies], orderBy: q => q.OrderBy(c => c.Name)) ;
            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            //// var category = context.Categories
            //     .Include(c => c.Movies)
            //     .FirstOrDefault(c => c.Id == id);
            var category =await _categoryRepository.GetOneAsync(c => c.Id == id, includes: [c => c.Movies]);
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
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            category.Name = category.Name.Trim();
           // context.Categories.Add(category);
           await _categoryRepository.AddAsync(category);
           // context.SaveChanges();
          await _categoryRepository.CommitAsync();

            TempData["Success"] = $"Category \"{category.Name}\" was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //var category = context.Categories.Find(id);
            var category =await _categoryRepository.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category input)
        {

            //   var category = context.Categories.Find(input.Id);
            var category =await _categoryRepository.FindAsync(input.Id);
            if (category == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            category.Name = input.Name.Trim();
            // context.SaveChanges();
           await _categoryRepository.CommitAsync();
            TempData["Success"] = $"Category \"{category.Name}\" was updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            //var category = context.Categories
            //    .Include(c => c.Movies)
            //    .FirstOrDefault(c => c.Id == id);
            var category =await _categoryRepository.GetOneAsync(c => c.Id == id, includes: [c => c.Movies]);
            if (category == null)
                return NotFound();

            var categoryName = category.Name;

            //context.Categories.Remove(category);
            //context.SaveChanges();
            _categoryRepository.Remove(category);
           await _categoryRepository.CommitAsync();

            TempData["Success"] = $"Category \"{categoryName}\" was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}