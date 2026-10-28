namespace Cinema.Repository
{
    public interface IMovieActorRepository : IRepository<MovieActor>
    {
        void RemoveRange(IEnumerable<MovieActor> movieActors);
    }
}
