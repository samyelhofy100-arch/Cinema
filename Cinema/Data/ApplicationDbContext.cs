namespace Cinema.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Cinemas> Cinemas => Set<Cinemas>();
        public DbSet<Actor> Actors => Set<Actor>();
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<MovieSubImage> MovieSubImages => Set<MovieSubImage>();
        public DbSet<MovieActor> MovieActors => Set<MovieActor>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(
                "Data Source=.;Initial Catalog=Cinema;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True");
        }

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
        }
    }
}