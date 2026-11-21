using Cinema.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Cinema.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Cinemas> Cinemas => Set<Cinemas>();
        public DbSet<Actor> Actors => Set<Actor>();
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<MovieSubImage> MovieSubImages => Set<MovieSubImage>();
        public DbSet<MovieActor> MovieActors => Set<MovieActor>();
        public DbSet<ApplicationUserOtp> ApplicationUserOtps { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MovieActor>()
                .HasKey(ma => new { ma.MovieId, ma.ActorId });

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Movie).WithMany()
                .HasForeignKey(i => i.MovieId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Showtime>()
                .HasOne(s => s.Hall).WithMany(h => h.Showtimes)
                .HasForeignKey(s => s.HallId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Showtime>()
                .HasOne(s => s.Movie).WithMany()
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Seat>()
                .HasIndex(s => new { s.HallId, s.Row, s.Number })
                .IsUnique();

            modelBuilder.Entity<CartItem>(e =>
            {
                e.HasIndex(c => new { c.ShowtimeId, c.SeatId }).IsUnique();
                e.HasOne(c => c.Seat).WithMany().HasForeignKey(c => c.SeatId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(c => c.Showtime).WithMany().HasForeignKey(c => c.ShowtimeId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Ticket>(e =>
            {
                e.HasIndex(t => new { t.ShowtimeId, t.SeatId }).IsUnique();
                e.HasOne(t => t.Seat).WithMany().HasForeignKey(t => t.SeatId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(t => t.Showtime).WithMany().HasForeignKey(t => t.ShowtimeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(t => t.Order).WithMany(o => o.Tickets).HasForeignKey(t => t.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}