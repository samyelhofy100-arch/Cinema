using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Helpers
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }

    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext context;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<DbInitializer> logger;

        public DbInitializer(ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<DbInitializer> logger)
        {
            this.context = context;
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task InitializeAsync()
        {
            // 1) apply pending migrations
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
                await context.Database.MigrateAsync();

            // 2) roles
            foreach (var role in new[] { SD.SuperAdmin, SD.Admin, SD.Customer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 3) super admin  (غيّر البيانات دي قبل ما ترفع المشروع على النت)
            const string email = "superadmin@cinema.com";
            const string password = "SuperAdmin@123";

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var admin = new ApplicationUser
                {
                    Name = "Super Admin",
                    UserName = "superadmin",
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, SD.SuperAdmin);
                else
                    logger.LogError("Could not create the super admin: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}