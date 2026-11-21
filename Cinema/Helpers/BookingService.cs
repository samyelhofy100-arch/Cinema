using Cinema.Data;
using Cinema.Models;
using Cinema.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Helpers
{
    public class BookingService
    {
        public const int HoldMinutes = 10;      // مدة الحجز في السلة
        public const int MaxSeats = 8;          // أقصى عدد كراسي للموعد الواحد
        public const int PaymentMinutes = 32;   // مدة حجز الكراسي أثناء الدفع (Stripe جلسته 31 دقيقة)

        private readonly ApplicationDbContext db;
        public BookingService(ApplicationDbContext db) { this.db = db; }

        // تنضيف الحجوزات المنتهية (بيتنادى قبل أي قراءة)
        public async Task CleanupAsync()
        {
            var now = DateTime.Now;

            await db.CartItems.Where(c => c.ExpiresAt < now).ExecuteDeleteAsync();

            var expired = await db.Orders
                .Where(o => o.Status == OrderStatus.Pending && o.ExpiresAt < now)
                .Select(o => o.Id).ToListAsync();

            if (expired.Count > 0)
            {
                await db.Tickets.Where(t => expired.Contains(t.OrderId)).ExecuteDeleteAsync();
                await db.Orders.Where(o => expired.Contains(o.Id))
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Expired));
            }
        }

        // إلغاء طلب لسه Pending وتحرير كراسيه
        public async Task CancelPendingAsync(Order order)
        {
            if (order.Status != OrderStatus.Pending) return;
            await db.Tickets.Where(t => t.OrderId == order.Id).ExecuteDeleteAsync();
            await db.Orders.Where(o => o.Id == order.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Cancelled));
        }

        // مواعيد الفيلم في الـ 7 أيام الجاية
        public async Task<List<ShowtimeSlotVM>> GetSlotsAsync(int movieId)
        {
            await CleanupAsync();

            var now = DateTime.Now;
            var until = now.Date.AddDays(8);

            var showtimes = await db.Showtimes
                .Include(s => s.Hall).ThenInclude(h => h.Cinema)
                .Where(s => s.MovieId == movieId && s.StartTime > now && s.StartTime < until)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var ids = showtimes.Select(s => s.Id).ToList();

            var tickets = await db.Tickets.Where(t => ids.Contains(t.ShowtimeId))
                .GroupBy(t => t.ShowtimeId).Select(g => new { Id = g.Key, N = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.N);

            var held = await db.CartItems.Where(c => ids.Contains(c.ShowtimeId))
                .GroupBy(c => c.ShowtimeId).Select(g => new { Id = g.Key, N = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.N);

            return showtimes.Select(s => new ShowtimeSlotVM
            {
                ShowtimeId = s.Id,
                CinemaName = s.Hall.Cinema.Name,
                HallName = s.Hall.Name,
                StartTime = s.StartTime,
                Price = s.Price,
                SeatsLeft = s.Hall.Rows * s.Hall.SeatsPerRow
                            - (tickets.TryGetValue(s.Id, out var t) ? t : 0)
                            - (held.TryGetValue(s.Id, out var h) ? h : 0)
            }).ToList();
        }
    }
}