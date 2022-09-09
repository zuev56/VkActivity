namespace VkActivity.Worker.Models
{
    public static class Constants
    {
        // TODO: Remove Unused
        public const string NoInernetConnection = nameof(NoInernetConnection);
        public const string EndDateIsNotMoreThanStartDate = nameof(EndDateIsNotMoreThanStartDate);
        public const string NoUsersInDatabase = nameof(NoUsersInDatabase);
        public const string SetUndefinedActivityToAllUsers = nameof(SetUndefinedActivityToAllUsers);
        public const string ActivityLogIsEmpty = nameof(ActivityLogIsEmpty);

        public const string GetFullTimeActivityError = nameof(GetFullTimeActivityError);
        public const string GetUsersError = nameof(GetUsersError);
        public const string GetUserStatisticsForPeriodError = nameof(GetUserStatisticsForPeriodError);
        public const string GetUsersWithActivityError = nameof(GetUsersWithActivityError);
        public const string SaveUsersActivityError = nameof(SaveUsersActivityError);
        public const string SetUndefinedActivityToAllUsersError = nameof(SetUndefinedActivityToAllUsersError);

        // TODO: move to other class
        public static string ActivityForUserNotFound(int userId) => $"ActivityForUser_{userId}_NotFound";
        public static string LoggedItemsCount(int count) => $"LoggedItemsCount: {count}";
        public static string UserNotFound(int userId) => $"User_{userId}_NotFound";
    }
}
