
namespace Cinema.Models
{
    public class Actor
    {
        public int Id { get; set; }
        [Required ,MaxLength(100)]
        public string Name { get; set; }
        public string? Img { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

    }
}
