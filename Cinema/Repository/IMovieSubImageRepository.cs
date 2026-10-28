namespace Cinema.Repository
{
    public interface IMovieSubImageRepository : IRepository<MovieSubImage>
    {
       void RemoveRange(IEnumerable<MovieSubImage> movieSubImage);
    }
}
