using System.Linq.Expressions;

namespace VkActivity.Data.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, int? skip = null, int? take = null, CancellationToken cancellationToken = default);
        Task<List<TEntity>> FindAllBySqlAsync(string sql, CancellationToken cancellationToken = default);
        Task<bool> SaveAsync<TId>(TEntity item, Func<TEntity, TId> getId, Action<TId> setId, CancellationToken cancellationToken = default);
        Task<bool> SaveRangeAsync<TId>(IEnumerable<TEntity> items, Func<TEntity, TId> getId, Action<TEntity, TId> setId, CancellationToken cancellationToken = default);
    }
}