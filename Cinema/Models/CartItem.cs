namespace Cinema.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; } = null!;
        public int SeatId { get; set; }
        public Seat Seat { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}