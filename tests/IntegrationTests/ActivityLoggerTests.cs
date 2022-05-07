using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Service;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Services;
using Xunit;

namespace IntegrationTests;

public class ActivityLoggerTests
{
    private const int _dbEntitiesAmount = 100;


    [Fact]
    public async Task SaveVkUsersActivityAsync_ReturnsSuccess()
    {
        // Arrange
        var activityLogger = GetActivityLogger();

        // Act
        var saveActivityResult = await activityLogger.SaveVkUsersActivityAsync();

        // Assert
        Assert.True(saveActivityResult?.IsSuccess);
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
    }

    private IActivityLogger GetActivityLogger()
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(_dbEntitiesAmount);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.GetFullPath(Constants.VkActivityServiceAppSettingsPath))
            .Build();

        return new ActivityLogger(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.VkUsersRepository,
            new VkIntegration(configuration[AppSettings.Vk.AccessToken], configuration[AppSettings.Vk.Version]),
            Mock.Of<ILogger<ActivityLogger>>());
    }
}
