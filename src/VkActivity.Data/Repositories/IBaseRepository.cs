using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace VkActivity.Data.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, int? skip = null, int? take = null, CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAllBySqlAsync(string sql, CancellationToken cancellationToken = default);
    Task<bool> SaveAsync(TEntity item, CancellationToken cancellationToken = default);
    Task<bool> SaveRangeAsync(IEnumerable<TEntity> items, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync<TId>(TEntity item, Func<TEntity, TId> getId, CancellationToken cancellationToken = default);
    Task<bool> UpdateRangeAsync<TId>(IEnumerable<TEntity> items, Func<TEntity, TId> getId, CancellationToken cancellationToken = default);
}