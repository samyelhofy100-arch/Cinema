using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace Cinema.Models
{

    public enum OrderStatus { Pending, Paid, Cancelled, Expired }

    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpiresAt { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [Precision(18, 2)] public decimal TotalAmount { get; set; }
        public string? StripeSessionId { get; set; }
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}