
using System.Security.Claims;
using Cinema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public CartCountViewComponent(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;

            if (User.Identity?.IsAuthenticated == true)
            {
                var uid = UserClaimsPrincipal.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

                var now = DateTime.Now;

                count = await db.CartItems.CountAsync(
                    c => c.UserId == uid && c.ExpiresAt > now
                );
            }

            return View(count);
        }
    }
}
