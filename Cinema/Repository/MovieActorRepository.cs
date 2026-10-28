namespace Cinema.Repository
{
    public class MovieActorRepository : Repository<MovieActor> , IMovieActorRepository
    {
        public MovieActorRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void RemoveRange(IEnumerable<MovieActor> movieActor)
        {
            _context.RemoveRange(movieActor); 
        }

    }
}
