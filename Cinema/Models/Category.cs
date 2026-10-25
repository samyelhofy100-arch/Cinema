
namespace Cinema.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required,MaxLength(100)]
        public string Name { get; set; }
        public ICollection<Movie>Movies { get; set; }=new List<Movie>();
    }
}
