using Microsoft.EntityFrameworkCore;

namespace Cinema.Models
{
    public class Showtime
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public int HallId { get; set; }
        public Hall Hall { get; set; } = null!;

        public DateTime StartTime { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }
    }
}