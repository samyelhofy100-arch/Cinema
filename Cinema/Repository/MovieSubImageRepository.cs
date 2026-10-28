namespace Cinema.Repository
{
    public class MovieSubImageRepository : Repository<MovieSubImage> , IMovieSubImageRepository
    {
        public MovieSubImageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void RemoveRange(IEnumerable<MovieSubImage> movieSubImage)
        {
            _context.RemoveRange(movieSubImage);
        }
    }
}
