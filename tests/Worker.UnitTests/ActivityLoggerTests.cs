﻿using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Services;
using Xunit;
using Zs.Common.Services.Logging.DelayedLogger;

namespace UnitTests;

public class ActivityLoggerTests
{
    private const int _dbEntitiesAmount = 1000;
    private readonly UserIdSet _userIdSet = UserIdSet.Create(_dbEntitiesAmount);

    [Fact]
    public async Task SaveVkUsersActivityAsync_ReturnsSuccess()
    {
        // Arrange
        var activityLoggerService = await GetActivityLoggerAsync(_userIdSet);

        // Act
        var saveActivityResult = await activityLoggerService.SaveUsersActivityAsync();

        // Assert
        Assert.True(saveActivityResult?.Successful);
        //Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
    }

    [Fact]
    public async Task SaveVkUsersActivityAsync_VkIntegrationFailed_ReturnsError()
    {
        // Arrange
        var activityLoggerService = await GetActivityLoggerAsync(_userIdSet, vkIntergationWorks: false);

        // Act
        var saveActivityResult = await activityLoggerService.SaveUsersActivityAsync();

        // Assert
        Assert.False(saveActivityResult?.Successful);
        //Assert.NotEmpty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Error));
        //Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
        //Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Info));
    }

    internal async Task<IActivityLogger> GetActivityLoggerAsync(UserIdSet userIdSet, bool vkIntergationWorks = true)
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        await postgreSqlInMemory.FillWithFakeDataAsync(userIdSet.InitialUsersAmount);

        var vkIntegrationMock = StubFactory.CreateVkIntegrationMock(userIdSet, vkIntergationWorks);

        return new ActivityLogger(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.UsersRepository,
            vkIntegrationMock.Object,
            Mock.Of<ILogger<ActivityLogger>>(),
            Mock.Of<IDelayedLogger<ActivityLogger>>());
    }

}