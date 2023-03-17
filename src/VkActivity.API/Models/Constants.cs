namespace VkActivity.Api.Models;

public static class Constants
{
    // TODO: Remove Unused
    public const string EndDateIsNotMoreThanStartDate = nameof(EndDateIsNotMoreThanStartDate);
    public const string GetUsersError = nameof(GetUsersError);
    public const string GetUserStatisticsForPeriodError = nameof(GetUserStatisticsForPeriodError);
    public const string GetUsersWithActivityError = nameof(GetUsersWithActivityError);

    // TODO: move to other class
    public static string ActivityForUserNotFound(int userId) => $"ActivityForUser_{userId}_NotFound";
    public static string UserNotFound(int userId) => $"User_{userId}_NotFound";
}