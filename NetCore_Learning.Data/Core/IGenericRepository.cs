using NetCore_Learning.Data.Models;
using NetCore_Learning.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Core
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByIdAsync(object id);
        Task AddAsync(T entity, bool autoCommit = true);
        Task AddRangeAsync(IEnumerable<T> entities, bool autoCommit = true);
        Task UpdateAsync(T entity, bool autoCommit = true);
        Task DeleteAsync(T entity, bool autoCommit = true);
        Task DeleteRangeAsync(IEnumerable<T> entities, bool autoCommit = true);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        IQueryable<T> Query(Expression<Func<T, bool>> predicate = null);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        Task<(IList<T> Data, int Total)> GetPagedAsync<TSelect>(
            Expression<Func<T, bool>> predicate,
            int pageIndex,
            int pageSize,
            Expression<Func<T, TSelect>> orderBy,
            bool asc = true);

        Task SaveAsync();
    }

}
