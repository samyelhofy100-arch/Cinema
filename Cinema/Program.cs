using Cinema.Data;
using Cinema.Models;
using Cinema.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Cinema.Utilities;

namespace Cinema
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                   .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IRepository<Actor>, Repository<Actor>>();
            builder.Services.AddScoped<IRepository<Category>, Repository<Category>>();
            builder.Services.AddScoped<IRepository<Cinemas>, Repository<Cinemas>>();
            builder.Services.AddScoped<IRepository<Movie>, Repository<Movie>>();
            builder.Services.AddScoped<IRepository<ApplicationUserOtp>, Repository<ApplicationUserOtp>>();

            builder.Services.AddScoped<IMovieActorRepository, MovieActorRepository>();
            builder.Services.AddScoped<IMovieSubImageRepository, MovieSubImageRepository>();

            builder.Services.AddTransient<IEmailSender, EmailSender>();

            var app = builder.Build();

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

            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Identity}/{controller=Account}/{action=Register}/{id?}");

           

            app.Run();
        }
    }
}
