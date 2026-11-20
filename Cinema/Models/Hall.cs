using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Hall
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int CinemaId { get; set; }
        public Cinemas Cinema { get; set; } = null!;

        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}