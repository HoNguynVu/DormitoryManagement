using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DormitoryDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(DormitoryDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Read operations
        public virtual async Task<T?> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Write operations
        public virtual void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        // Count operations
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null 
                ? await _dbSet.CountAsync() 
                : await _dbSet.CountAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}