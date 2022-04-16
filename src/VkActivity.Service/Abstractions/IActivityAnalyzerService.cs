using VkActivity.Data.Models;
using VkActivity.Service.Models;
using Zs.Common.Abstractions;

namespace VkActivity.Service.Abstractions;

public interface IActivityAnalyzerService
{
    /// <summary>Get information about user activity in the specified period</summary>
    Task<IOperationResult<SimpleUserActivity>> GetUserStatisticsForPeriodAsync(int userId, DateTime fromDate, DateTime toDate);

    /// <summary>Get detailed information about full user activity</summary>
    Task<IOperationResult<DetailedUserActivity>> GetFullTimeUserStatisticsAsync(int userId);

    /// <summary>Get users list </summary>
    Task<IOperationResult<List<User>>> GetUsersAsync(string filterText = null, int? skip = null, int? take = null);

    /// <summary>Get users list with activity time in a specified period</summary>
    Task<IOperationResult<List<UserWithActivity>>> GetUsersWithActivityAsync(string filterText, DateTime fromDate, DateTime toDate);

    ///// <summary> Get paginated users list with activity time in a specified period </summary>
    ///// <param name="filterText">Filter for user names</param>
    ///// <param name="fromDate">Start of period</param>
    ///// <param name="toDate">End of period</param>
    //Task<IOperationResult<Table<UserWithActivity>>> GetUsersWithActivityTable(TableParameters requestParameters);

}
