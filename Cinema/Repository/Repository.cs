using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Cinema.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context; //= new ApplicationDbContext();
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<EntityEntry<T>> AddAsync(T entity)
        {
            return await _dbSet.AddAsync(entity);
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
        {
            return await Query(filter).CountAsync();
        }
        private IQueryable<T> Query(
                Expression<Func<T, bool>>? filter = null,
                Expression<Func<T, object>>[]? includes = null,
                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                    int? skip = null,
                    int? take = null,
                    string[]? includeProperties = null,
                bool isTracked = true
            )
        {
            var entities = _dbSet.AsQueryable();
            if (filter != null)
                entities = entities.Where(filter);
            if (includes != null)
            {
                foreach (var include in includes)
                    entities = entities.Include(include);
            }
            if (orderBy != null)
                entities = orderBy(entities);
            if (!isTracked)
            {
                entities = entities.AsNoTracking();
            }
            if (skip.HasValue)
                entities = entities.Skip(skip.Value);
            if (take.HasValue)
                entities = entities.Take(take.Value);
            if (includeProperties != null)
            {
                foreach (var path in includeProperties)
                    entities = entities.Include(path);
            }
            return entities;
        }
        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>[]? includes = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null,
            string[]? includeProperties = null,

            bool isTracked = true
            )
        {
            var entities = Query(filter, includes, orderBy, skip, take, includeProperties, isTracked);

            return await entities.ToListAsync();
        }
        public async Task<T> GetOneAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>[]? includes = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null,
            string[]? includeProperties = null,

            bool isTracked = true
            )
        {
            var entities = Query(filter, includes, orderBy, skip, take, includeProperties, isTracked);
            return await entities.FirstOrDefaultAsync();
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }
        public async Task<T> FindAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.AnyAsync(filter);
        }
        public async Task<int> CommitAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

    }
}
