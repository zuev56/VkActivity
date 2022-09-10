namespace VkActivity.Common;

public static class AppSettings
{
    public const string Kestrel = nameof(Kestrel);
    public static class ConnectionStrings
    {
        public const string Default = nameof(Default);
    }

    public static class Vk
    {
        public const string Version = $"{nameof(Vk)}:{nameof(Version)}";
        public const string AccessToken = $"{nameof(Vk)}:{nameof(AccessToken)}";
        public const string InitialUserIds = $"{nameof(Vk)}:{nameof(InitialUserIds)}";
        public const string ActivityLogIntervalSec = $"{nameof(Vk)}:{nameof(ActivityLogIntervalSec)}";
        public const string UsersDataUpdateIntervalHours = $"{nameof(Vk)}:{nameof(UsersDataUpdateIntervalHours)}";
    }

    public static class ConnectionAnalyser
    {
        public const string Urls = $"{nameof(ConnectionAnalyser)}:{nameof(Urls)}";
    }

    public static class Swagger
    {
        public const string ApiTitle = $"{nameof(Swagger)}:{nameof(ApiTitle)}";
        public const string ApiVersion = $"{nameof(Swagger)}:{nameof(ApiVersion)}";
        public const string EndpointUrl = $"{nameof(Swagger)}:{nameof(EndpointUrl)}";
    }

    public static class Proxy
    {
        public const string UseProxy = $"{nameof(Proxy)}:{nameof(UseProxy)}";
        public const string Socket = $"{nameof(Proxy)}:{nameof(Socket)}";
        public const string Login = $"{nameof(Proxy)}:{nameof(Login)}";
        public const string Password = $"{nameof(Proxy)}:{nameof(Password)}";
    }
}
