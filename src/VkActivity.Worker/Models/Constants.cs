namespace VkActivity.Worker.Models
{
    public static class Constants
    {
        public const string NoInernetConnection = nameof(NoInernetConnection);
        public const string NoUsersInDatabase = nameof(NoUsersInDatabase);
        public const string SetUndefinedActivityToAllUsers = nameof(SetUndefinedActivityToAllUsers);
        public const string ActivityLogIsEmpty = nameof(ActivityLogIsEmpty);
        public const string SaveUsersActivityError = nameof(SaveUsersActivityError);
        public const string SetUndefinedActivityToAllUsersError = nameof(SetUndefinedActivityToAllUsersError);

        // TODO: move to other class
        public static string LoggedItemsCount(int count) => $"LoggedItemsCount: {count}";
    }
}
