using Cinema.Models;

namespace Cinema.ViewModels
{
    public class ShowtimeSlotVM
    {
        public int ShowtimeId { get; set; }
        public string CinemaName { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public decimal Price { get; set; }
        public int SeatsLeft { get; set; }
    }

    public class SeatMapVM
    {
        public Showtime Showtime { get; set; } = null!;
        public List<Seat> Seats { get; set; } = new();
        public HashSet<int> Taken { get; set; } = new();   // محجوزة (تذكرة أو عند حد تاني)
        public HashSet<int> Mine { get; set; } = new();    // محجوزة ليك في السلة
    }
}