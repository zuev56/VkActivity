using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Services;
using Worker.UnitTests.Data;
using Xunit;
using Zs.Common.Services.Logging.DelayedLogger;

namespace Worker.UnitTests;

public sealed class ActivityLoggerTests
{
    private const int DbEntitiesAmount = 1000;
    private readonly UserIdSet _userIdSet = UserIdSet.Create(DbEntitiesAmount);

    [Fact]
    public async Task SaveVkUsersActivityAsync_ReturnsSuccess()
    {
        var activityLoggerService = await GetActivityLoggerAsync(_userIdSet);

        var saveActivityResult = await activityLoggerService.SaveUsersActivityAsync();

        Assert.True(saveActivityResult.Successful);
        //Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
    }

    [Fact]
    public async Task SaveVkUsersActivityAsync_VkIntegrationFailed_ReturnsError()
    {
        var activityLoggerService = await GetActivityLoggerAsync(_userIdSet, vkIntegrationWorks: false);

        var saveActivityResult = await activityLoggerService.SaveUsersActivityAsync();

        Assert.False(saveActivityResult.Successful);
        //Assert.NotEmpty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Error));
        //Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
        //Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Info));
    }

    internal async Task<IActivityLogger> GetActivityLoggerAsync(UserIdSet userIdSet, bool vkIntegrationWorks = true)
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        await postgreSqlInMemory.FillWithFakeDataAsync(userIdSet.InitialUsersAmount);

        var vkIntegrationMock = StubFactory.CreateVkIntegrationMock(userIdSet, vkIntegrationWorks);

        return new ActivityLogger(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.UsersRepository,
            vkIntegrationMock.Object,
            Mock.Of<ILogger<ActivityLogger>>(),
            Mock.Of<IDelayedLogger<ActivityLogger>>());
    }
}