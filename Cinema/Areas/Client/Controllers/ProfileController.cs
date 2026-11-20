
using Cinema.Models;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize]
    public class ProfileController : Controller
    {
        private const string Folder = "UserImg";

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IWebHostEnvironment env;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment env)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            return View(new ProfileVM
            {
                FullName = user.Name,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ExistingImg = user.Img,
                UserName = user.UserName ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileVM vm)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var oldImg = user.Img;

            vm.ExistingImg = oldImg;
            vm.UserName = user.UserName ?? string.Empty;

            string? newImg = null;

            if (vm.ImgFile is { Length: > 0 })
            {
                var extension = Path.GetExtension(vm.ImgFile.FileName).ToLowerInvariant();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(vm.ImgFile),
                        "Only JPG, JPEG, PNG and WEBP images are allowed.");
                }

                if (vm.ImgFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(vm.ImgFile),
                        "Image size must not exceed 5 MB.");
                }
            }

            if (!ModelState.IsValid)
                return View(vm);

            user.Name = vm.FullName.Trim();
            user.Email = vm.Email.Trim();
            user.PhoneNumber = vm.PhoneNumber;
            user.Address = vm.Address;

            if (vm.ImgFile is { Length: > 0 })
            {
                var folderPath = Path.Combine(env.WebRootPath, Folder);

                Directory.CreateDirectory(folderPath);

                newImg = Guid.NewGuid().ToString("N") +
                         Path.GetExtension(vm.ImgFile.FileName).ToLowerInvariant();

                var filePath = Path.Combine(folderPath, newImg);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImgFile.CopyToAsync(stream);
                }

                user.Img = newImg;
            }

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                DeleteImage(newImg);

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(vm);
            }

            if (newImg != null)
                DeleteImage(oldImg);

            await signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Profile updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM vm)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ",
                    ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                return RedirectToAction(nameof(Index));
            }

            var result = await userManager.ChangePasswordAsync(
                user, vm.CurrentPassword, vm.NewPassword);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ",
                    result.Errors.Select(e => e.Description));

                return RedirectToAction(nameof(Index));
            }

            await signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Password changed successfully.";

            return RedirectToAction(nameof(Index));
        }

        private void DeleteImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var safeFileName = Path.GetFileName(fileName);
            var filePath = Path.Combine(env.WebRootPath, Folder, safeFileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }
}
