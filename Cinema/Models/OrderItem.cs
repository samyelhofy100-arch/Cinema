// Models/OrderItem.cs
using Microsoft.EntityFrameworkCore;

namespace Cinema.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public int Count { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }   // سعر التذكرة وقت الشراء
    }
}