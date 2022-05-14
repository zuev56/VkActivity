using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;
using Zs.Common.Extensions;
using Zs.Common.Models;

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
        => await base.FindAllAsync(skip: skip, take: take, cancellationToken: cancellationToken);

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

    public async Task<ServiceResult> UpdateRangeAsync(IEnumerable<User> users, CancellationToken cancellationToken)
    {
        var userIds = users.Select(u => u.Id);
        await using (var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbUsers = context.VkUsers!.Where(u => userIds.Contains(u.Id));

            foreach (var dbUser in dbUsers)
            {
                var user = users.First(u => u.Id == dbUser.Id);
                if (!dbUser.Equals(user))
                {
                    UpdateUserFromOther(dbUser, user);
                }
            }

            var saved = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return ServiceResult.Success();
        }
    }

    private void UpdateUserFromOther(User target, User source)
    {
        var rawDataHistory = target.RawDataHistory != null
            ? JsonNode.Parse(target.RawDataHistory).AsArray()
            : new JsonArray();

        var historyItem = JsonNode.Parse(target.RawData).AsObject();
        historyItem.Add("addedToHistory", DateTime.UtcNow);
        rawDataHistory.Add(historyItem);
        target.RawDataHistory = JsonSerializer.Serialize(rawDataHistory).NormalizeJsonString();

        target.FirstName = source.FirstName;
        target.LastName = source.LastName;
        target.RawData = source.RawData;
        target.UpdateDate = DateTime.UtcNow;
    }
}
