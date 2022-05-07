using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;

namespace VkActivity.Data.Repositories;

public sealed class UsersRepository : BaseRepository<VkActivityContext, User>, IUsersRepository
{
    public UsersRepository(
        IDbContextFactory<VkActivityContext> contextFactory,
        TimeSpan? criticalQueryExecutionTimeForLogging = null,
        ILoggerFactory? loggerfFactory = null)
        : base(contextFactory, criticalQueryExecutionTimeForLogging, loggerfFactory)
    {
    }

    public async Task<List<User>> FindAllWhereNameLikeValueAsync(string value, int? skip, int? take, CancellationToken cancellationToken = default)
        => await FindAllAsync(
               u => EF.Functions.ILike(u.FirstName!, $"%{value}%") || EF.Functions.ILike(u.LastName!, $"%{value}%"),
               skip: skip,
               take: take,
               cancellationToken: cancellationToken);

    public async Task<List<User>> FindAllByIdsAsync(params int[] userIds)
        => await FindAllAsync(u => userIds.Contains(u.Id));

    public async Task<User?> FindByIdAsync(int userId, CancellationToken cancellationToken = default)
        => await FindAsync(u => u.Id == userId, cancellationToken: cancellationToken).ConfigureAwait(false);

    public async Task<List<User>> FindAllAsync(int? skip, int? take, CancellationToken cancellationToken = default)
        => await FindAllAsync(skip: skip, take: take, cancellationToken: cancellationToken);

    public async Task<int[]> FindAllIdsAsync(CancellationToken cancellationToken = default)
    {
        await using (var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            return await context.VkUsers!
                .Select(u => u.Id).ToArrayAsync()
                .ConfigureAwait(false);
        }
    }

    public async Task<int[]> FindExistingIdsAsync(int[] userIds, CancellationToken cancellationToken = default)
    {
        await using (var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            return await context.VkUsers!
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Id).ToArrayAsync()
                .ConfigureAwait(false);
        }
    }
}
