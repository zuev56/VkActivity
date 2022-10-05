using System.Diagnostics.CodeAnalysis;

namespace Worker.IntegrationTests;

[ExcludeFromCodeCoverage]
internal static class Constants
{
    public const string VkActivityServiceAppSettingsPath
        = "../../../../../src/VkActivity.Worker/appsettings.Development.json";
    public const string DbUserSecretsKey = "ConnectionString:User";
    public const string DbPasswordSecretsKey = "ConnectionString:Password";
    public const string DbPortSecretsKey = "ConnectionString:Port";
    public const string DeletedUserName = "DELETED";

}
