
using Cinema.Utilities;
using Cinema.Helpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace Cinema
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Stripe
            Stripe.StripeConfiguration.ApiKey =
                builder.Configuration["Stripe:SecretKey"];

            // Database Connection
            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = true;

                options.Password.RequiredLength = 6;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(10);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Authentication Cookie
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Client/Account/Login";
                options.AccessDeniedPath = "/Client/Account/AccessDenied";
            });

            // Security Stamp
            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
            });

            // MVC
            builder.Services.AddControllersWithViews();

            // Repositories
            builder.Services.AddScoped(
                typeof(IRepository<>),
                typeof(Repository<>));

            builder.Services.AddScoped<IMovieActorRepository, MovieActorRepository>();
            builder.Services.AddScoped<IMovieSubImageRepository, MovieSubImageRepository>();

            // Email
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            // Database Initializer
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();

            builder.Services.AddScoped<Cinema.Helpers.BookingService>();

            var app = builder.Build();

            // Database Seeding
            using (var scope = app.Services.CreateScope())
            {
                var initializer =
                    scope.ServiceProvider.GetRequiredService<IDbInitializer>();

                await initializer.InitializeAsync();
            }

            // Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            // Default Route
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Identity}/{controller=Account}/{action=Register}/{id?}");

            await app.RunAsync();
        }
    }
}
