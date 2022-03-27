using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;

namespace VkActivity.Data.Repositories;

public class BaseRepository<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class
{
    private readonly TimeSpan _criticalQueryExecutionTime;
    private readonly ILogger<BaseRepository<TContext, TEntity>>? _logger;
    protected IDbContextFactory<TContext> ContextFactory { get; }

    public BaseRepository(
        IDbContextFactory<TContext> contextFactory,
        TimeSpan? criticalQueryExecutionTimeForLogging = null,
        ILoggerFactory? loggerfFactory = null)
    {
        ContextFactory = contextFactory ?? throw new NullReferenceException(nameof(contextFactory));
        _criticalQueryExecutionTime = criticalQueryExecutionTimeForLogging ?? TimeSpan.FromSeconds(1);
        _logger = loggerfFactory?.CreateLogger<BaseRepository<TContext, TEntity>>();
    }

    /// <summary>
    /// Asynchronously returns the list of elements of a sequence that satisfies a specified condition
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="orderBy"> Sorting rules before executing predicate</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<List<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var sw = new Stopwatch();
        sw.Start();
        string? resultQuery = null;

        try
        {
            await using (var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                IQueryable<TEntity> query = context.Set<TEntity>();

                if (predicate != null)
                    query = query.Where(predicate);

                if (orderBy != null)
                    query = orderBy(query);

                if (skip != null)
                    query = query.Skip((int)skip);

                if (take != null)
                    query = query.Take((int)take);

                resultQuery = query.ToQueryString();

                return await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            sw.Stop();
            LogFind("Repository.FindAllAsync [Elapsed: {Elapsed}].\n\tSQL: {SQL}", sw.Elapsed, resultQuery);
        }
    }

    protected async Task<List<TEntity>> FindAllBySqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        var sw = new Stopwatch();
        sw.Start();
        string? resultQuery = null;

        try
        {
            await using (var context = await ContextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                var query = context.Set<TEntity>().FromSqlRaw(sql);

                resultQuery = query.ToQueryString();

                return await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            sw.Stop();
            LogFind("Repository.FindAllBySqlAsync [Elapsed: {Elapsed}].\n\tSQL: {SQL}", sw.Elapsed, resultQuery);
        }
    }

    protected void LogFind(string message, TimeSpan elapsed, string sql)
    {
        if (elapsed > _criticalQueryExecutionTime)
            _logger?.LogWarningIfNeed(message, elapsed, sql);
        else
            _logger?.LogDebugIfNeed(message, elapsed, sql);
    }

}
