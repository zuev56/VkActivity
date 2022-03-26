using VkActivity.Data.Models;

namespace Home.Data.Abstractions;

public interface IActivityLogItemsRepository
{
    Task<List<ActivityLogItem>> FindLastUsersActivity(params int[] userIds);
    Task<List<ActivityLogItem>> FindAllByIdsInDateRangeAsync(int[] userIds, DateTime fromDate, DateTime toDate);
    Task<bool> SaveRangeAsync(List<ActivityLogItem> activityLogItems);
}
