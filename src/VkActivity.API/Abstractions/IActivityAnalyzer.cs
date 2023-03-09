using VkActivity.Api.Models;
using VkActivity.Data.Models;
using Zs.Common.Abstractions;
using Zs.Common.Models;

namespace VkActivity.Api.Abstractions;

public interface IActivityAnalyzer
{
    /// <summary>Get information about user activity in the specified period</summary>
    Task<Result<DetailedActivity>> GetUserStatisticsForPeriodAsync(int userId, DateTime fromDate, DateTime toDate);

    /// <summary>Get users list </summary>
    Task<Result<List<User>>> GetUsersAsync(string filterText = null, int? skip = null, int? take = null);

    /// <summary>Get users list with activity time in a specified period</summary>
    Task<Result<List<ActivityListItem>>> GetUsersWithActivityAsync(DateTime fromDate, DateTime toDate, string? filterText);
    Task<Result<DateTime>> GetLastVisitDate(int userId);
    Task<Result<bool>> IsOnline(int userId);

    ///// <summary> Get paginated users list with activity time in a specified period </summary>
    ///// <param name="filterText">Filter for user names</param>
    ///// <param name="fromDate">Start of period</param>
    ///// <param name="toDate">End of period</param>
    //Task<Result<Table<UserWithActivity>>> GetUsersWithActivityPage(TableParameters requestParameters, string nextToken);

}