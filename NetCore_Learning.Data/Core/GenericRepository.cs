using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NetCore_Learning.Data.Configuration;
using NetCore_Learning.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Core
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public virtual async Task<T> GetByConditionAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        public virtual async Task<T> GetByIdAsync(object id)
            => await _dbSet.FindAsync(id);

        public virtual async Task AddAsync(T entity, bool autoCommit = true)
        {
            await _dbSet.AddAsync(entity);
            if (autoCommit)
                await _context.SaveChangesAsync();
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, bool autoCommit = true)
        {
            await _dbSet.AddRangeAsync(entities);
            if (autoCommit)
                await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity, bool autoCommit = true)
        {
            _dbSet.Update(entity);
            if (autoCommit)
                await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity, bool autoCommit = true)
        {
            _dbSet.Remove(entity);
            if (autoCommit)
                await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, bool autoCommit = true)
        {
            _dbSet.RemoveRange(entities);
            if (autoCommit)
                await _context.SaveChangesAsync();
        }

        // --- CHECK/QUERY ---
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        public virtual IQueryable<T> Query(Expression<Func<T, bool>> predicate = null)
            => predicate == null ? _dbSet : _dbSet.Where(predicate);

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
            => predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        // --- PAGINATION ---
        public virtual async Task<(IList<T> Data, int Total)> GetPagedAsync<TSelect>(
            Expression<Func<T, bool>> predicate,
            int pageIndex,
            int pageSize,
            Expression<Func<T, TSelect>> orderBy,
            bool asc = true)
        {
            var query = _dbSet.Where(predicate);
            var total = await query.CountAsync();

            var items = asc
                ? await query.OrderBy(orderBy)
                             .Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync()
                : await query.OrderByDescending(orderBy)
                             .Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();

            return (items, total);
        }

        // --- SAVE MANUAL ---
        public virtual async Task SaveAsync()
            => await _context.SaveChangesAsync();
    }


    public enum OrderType
    {
        Asc,
        Desc
    }

}
