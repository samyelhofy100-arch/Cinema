using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Cinema.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<EntityEntry<T>> AddAsync(T entity);
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>[]? includes = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null,
            string[]? includeProperties = null,
            bool isTracked = true);

        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>[]? includes = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null,
            string[]? includeProperties = null,
            bool isTracked = true);

        void Update(T entity);
        void Remove(T entity);
        Task<T?> FindAsync(int id);
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
        Task<int> CommitAsync();
    }
}