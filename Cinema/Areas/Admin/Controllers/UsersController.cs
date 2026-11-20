using Cinema.Areas.Admin.ViewModels;
using Cinema.Helpers;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.SuperAdmin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.UserName!.Contains(search)
                                      || u.Email!.Contains(search)
                                      || u.Name.Contains(search));

            var users = await query.OrderBy(u => u.UserName).ToListAsync();

            var rows = new List<UserRowVM>();
            foreach (var u in users)
            {
                var roles = await userManager.GetRolesAsync(u);
                rows.Add(new UserRowVM
                {
                    Id = u.Id,
                    FullName = u.Name,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "-",
                    IsLocked = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            ViewBag.Search = search;
            return View(rows);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUnlock(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await userManager.IsInRoleAsync(user, SD.SuperAdmin))
            {
                TempData["Error"] = "The Super Admin account cannot be locked.";
                return RedirectToAction(nameof(Index));
            }

            bool isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

            if (isLocked)
            {
                await userManager.SetLockoutEndDateAsync(user, null);
                await userManager.ResetAccessFailedCountAsync(user);
                TempData["Success"] = $"\"{user.UserName}\" was unlocked.";
            }
            else
            {
                await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                await userManager.UpdateSecurityStampAsync(user);   // يطلّعه لو كان مسجل دخول
                TempData["Success"] = $"\"{user.UserName}\" was locked.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string id, string role)
        {
            if (role != SD.Admin && role != SD.Customer) return BadRequest();

            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await userManager.IsInRoleAsync(user, SD.SuperAdmin))
            {
                TempData["Error"] = "The Super Admin role cannot be changed.";
                return RedirectToAction(nameof(Index));
            }

            var current = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, current);
            await userManager.AddToRoleAsync(user, role);
            await userManager.UpdateSecurityStampAsync(user);   // الصلاحية الجديدة تتطبق فوراً

            TempData["Success"] = $"\"{user.UserName}\" is now {role}.";
            return RedirectToAction(nameof(Index));
        }
    }
}