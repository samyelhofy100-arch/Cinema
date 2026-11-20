// Models/Order.cs
using Microsoft.EntityFrameworkCore;

namespace Cinema.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Precision(18, 2)]
        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}