using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Cinemas
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Img { get; set; }
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();


    }
}
