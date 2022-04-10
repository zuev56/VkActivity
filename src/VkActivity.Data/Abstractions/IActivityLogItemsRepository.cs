using VkActivity.Data.Models;
using VkActivity.Data.Repositories;

namespace VkActivity.Data.Abstractions;

public interface IActivityLogItemsRepository : IBaseRepository<ActivityLogItem>
{
    Task<List<ActivityLogItem>> FindLastUsersActivity(params int[] userIds);
    Task<List<ActivityLogItem>> FindAllByIdsInDateRangeAsync(int[] userIds, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}
