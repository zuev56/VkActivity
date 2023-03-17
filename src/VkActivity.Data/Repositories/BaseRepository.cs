using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;

namespace VkActivity.Data.Repositories;

public class BaseRepository<TContext, TEntity> : IBaseRepository<TEntity> where TContext : DbContext
    where TEntity : class
{
    private readonly TimeSpan _criticalQueryExecutionTime;
    private readonly ILogger<BaseRepository<TContext, TEntity>>? _logger;
    protected IDbContextFactory<TContext> ContextFactory { get; }

    protected BaseRepository(
        IDbContextFactory<TContext> contextFactory,
        TimeSpan? criticalQueryExecutionTimeForLogging = null,
        ILoggerFactory? loggerFactory = null)
    {
        ContextFactory = contextFactory;
        _criticalQueryExecutionTime = criticalQueryExecutionTimeForLogging ?? TimeSpan.FromSeconds(1);
        _logger = loggerFactory?.CreateLogger<BaseRepository<TContext, TEntity>>();
    }

    protected async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return predicate is null
            ? await context.Set<TEntity>().CountAsync(cancellationToken).ConfigureAwait(false)
            : await context.Set<TEntity>().CountAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a specified
    /// condition or a default value if no such element is found
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="orderBy"> Sorting rules before executing predicate</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var sw = new Stopwatch();
        sw.Start();
        string? resultQuery = null;

        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            IQueryable<TEntity> query = context.Set<TEntity>();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            resultQuery = query.ToQueryString();

            return await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            LogFind("Repository.FindAsync [Elapsed: {Elapsed}].\n\tSQL: {SQL}", sw.Elapsed, resultQuery ?? "");
        }
    }

    /// <summary>
    /// Asynchronously returns the list of elements of a sequence that satisfies a specified condition
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="orderBy"> Sorting rules before executing predicate</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<List<TEntity>> FindAllAsync(
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
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            IQueryable<TEntity> query = context.Set<TEntity>();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip != null)
            {
                query = query.Skip((int)skip);
            }

            if (take != null)
            {
                query = query.Take((int)take);
            }

            resultQuery = query.ToQueryString();

            return await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            LogFind("Repository.FindAllAsync [Elapsed: {Elapsed}].\n\tSQL: {SQL}", sw.Elapsed, resultQuery ?? "");
        }
    }

    public async Task<List<TEntity>> FindAllBySqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        var sw = new Stopwatch();
        sw.Start();
        string? resultQuery = null;

        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var query = context.Set<TEntity>().FromSqlRaw(sql);

            resultQuery = query.ToQueryString();

            return await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            LogFind("Repository.FindAllBySqlAsync [Elapsed: {Elapsed}].\n\tSQL: {SQL}", sw.Elapsed, resultQuery);
        }
    }

    public virtual async Task<bool> SaveAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        // TODO: Split into AddNewOrUpdateAsync AddNewAsync and UpdateExistingAsync
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        var sw = new Stopwatch();
        sw.Start();
        string? resultChanges = null;
        string? detailedResultChanges = null;

        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            context.Set<TEntity>().Add(item);

            GetChangesForLogging(context, out resultChanges, out detailedResultChanges);

            return 1 == await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            LogSave("Repository.SaveAsync [Elapsed: {Elapsed}].\n\tChanges: {changes}", sw.Elapsed, resultChanges ?? "", detailedResultChanges ?? "");
        }
    }

    private static void GetChangesForLogging(TContext context, out string? resultChanges, out string? detailedResultChanges)
    {
        resultChanges = Environment.NewLine + context.ChangeTracker.ToDebugString(ChangeTrackerDebugStringOptions.ShortDefault);
        detailedResultChanges = Environment.NewLine + context.ChangeTracker.ToDebugString(ChangeTrackerDebugStringOptions.LongDefault);
    }

    public virtual async Task<bool> SaveRangeAsync(IEnumerable<TEntity> items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        var sw = new Stopwatch();
        sw.Start();
        string? resultChanges = null;
        string? detailedResultChanges = null;

        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            foreach (var item in items)
            {
                context.Set<TEntity>().Add(item);
            }

            GetChangesForLogging(context, out resultChanges, out detailedResultChanges);

            return items.Count() == await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            LogSave("Repository.SaveRangeAsync [Elapsed: {Elapsed}].\n\tChanges: {changes}", sw.Elapsed, resultChanges, detailedResultChanges);
        }
    }

    private static async Task<TEntity> AddItemForSaveToContext<TId>(TContext context, TEntity item, Func<TEntity, TId> getId, CancellationToken cancellationToken)
    {
        var itemId = getId(item);

        var existingItem = itemId != null && !itemId.Equals(default(TId))
            ? await context.Set<TEntity>().FirstOrDefaultAsync(i => getId(i)!.Equals(itemId), cancellationToken).ConfigureAwait(false)
            : null;

        if (existingItem != null && !item.Equals(existingItem))
        {
            context.Entry(existingItem).State = EntityState.Detached;
            context.Entry(item).State = EntityState.Modified;
            context.Set<TEntity>().Update(item);
        }
        else
        {
            context.Set<TEntity>().Add(item);
        }

        return item;
    }

    public Task<bool> UpdateAsync<TId>(TEntity item, Func<TEntity, TId> getId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateRangeAsync<TId>(IEnumerable<TEntity> items, Func<TEntity, TId> getId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    //public virtual async Task<bool> DeleteAsync(TEntity item, CancellationToken cancellationToken = default)
    //{
    //    if (item is null)
    //        throw new ArgumentNullException(nameof(item));
    //
    //    var sw = new Stopwatch();
    //    sw.Start();
    //    string resultChanges = null;
    //
    //    try
    //    {
    //        await using (var context = await ContextFactory.CreateDbContextAsync().ConfigureAwait(false))
    //        {
    //            var existingItem = await context.Set<TEntity>().FirstOrDefaultAsync(i => i.Id.Equals(item.Id), cancellationToken).ConfigureAwait(false);
    //            if (existingItem != null)
    //            {
    //                context.Set<TEntity>().Remove(existingItem);
    //                resultChanges = context.ChangeTracker.ToDebugString(ChangeTrackerDebugStringOptions.LongDefault);
    //
    //                return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false) == 1;
    //            }
    //        }
    //
    //
    //        return false;
    //    }
    //    finally
    //    {
    //        sw.Stop();
    //        LogDelete("Repository.DeleteAsync [Elapsed: {Elapsed}].\n\tChanges: {changes}", sw.Elapsed, resultChanges);
    //    }
    //}
    //
    //public virtual async Task<bool> DeleteRangeAsync(IEnumerable<TEntity> items, CancellationToken cancellationToken = default)
    //{
    //    if (items is null)
    //        throw new ArgumentNullException(nameof(items));
    //
    //    var sw = new Stopwatch();
    //    sw.Start();
    //    string resultChanges = null;
    //
    //    try
    //    {
    //        await using (var context = await ContextFactory.CreateDbContextAsync().ConfigureAwait(false))
    //        {
    //            var ids = items.Select(i => i.Id).ToList();
    //            var existingItems = await context.Set<TEntity>().Where(i => ids.Contains(i.Id)).ToListAsync(cancellationToken);
    //            if (existingItems?.Any() == true && existingItems.Count == items.Count())
    //            {
    //                context.Set<TEntity>().RemoveRange(existingItems);
    //                resultChanges = context.ChangeTracker.ToDebugString(ChangeTrackerDebugStringOptions.LongDefault);
    //
    //                return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false) == existingItems.Count;
    //            }
    //        }
    //
    //        return false;
    //    }
    //    finally
    //    {
    //        sw.Stop();
    //        LogDelete("Repository.DeleteRangeAsync [Elapsed: {Elapsed}].\n\tChanges: {changes}", sw.Elapsed, resultChanges);
    //    }
    //}

    protected void LogFind(string message, TimeSpan elapsed, string sql)
    {
        if (elapsed > _criticalQueryExecutionTime)
        {
            _logger?.LogWarningIfNeed(message, elapsed, sql);
        }
        else
        {
            _logger?.LogDebugIfNeed(message, elapsed, sql);
        }
    }

    protected void LogSave(string message, TimeSpan elapsed, string changes, string detailedChanges)
    {
        if (elapsed > _criticalQueryExecutionTime)
        {
            _logger?.LogWarning(message, elapsed, detailedChanges);
        }
        else if (_logger?.IsEnabled(LogLevel.Trace) != true)
        {
            _logger?.LogDebug(message, elapsed, changes);
        }
        else
        {
            _logger?.LogTrace(message, elapsed, detailedChanges);
        }
    }

    protected void LogDelete(string message, TimeSpan elapsed, string changes)
    {
        LogFind(message, elapsed, changes);
    }
}