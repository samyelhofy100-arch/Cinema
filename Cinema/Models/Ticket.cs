using Microsoft.EntityFrameworkCore;

namespace Cinema.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; } = null!;
        public int SeatId { get; set; }
        public Seat Seat { get; set; } = null!;
        [Precision(18, 2)] public decimal Price { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}