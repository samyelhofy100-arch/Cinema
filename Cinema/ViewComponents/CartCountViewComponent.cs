using System.Security.Claims;
using Cinema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext context;

        public CartCountViewComponent(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;

            if (UserClaimsPrincipal.Identity?.IsAuthenticated == true)
            {
                var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                count = await context.Carts
                    .Where(c => c.ApplicationUserId == userId)
                    .SumAsync(c => c.Count);
            }

            return View(count);
        }
    }
}