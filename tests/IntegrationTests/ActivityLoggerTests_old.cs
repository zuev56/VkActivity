using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Data.Repositories;
using VkActivity.Worker;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Services;
using Xunit;

namespace IntegrationTests;

public class ActivityLoggerTests_old
{
    private const int _dbEntitiesAmount = 100;
    private readonly ActivityLogItemsRepository? _activityLogItemsRepository;
    private readonly UsersRepository? _usersRepository;


    [Fact]
    public async Task SaveVkUsersActivityAsync_ReturnsSuccess()
    {
        // Arrange
        var activityLogger = GetActivityLogger();

        // Act
        var saveActivityResult = await activityLogger.SaveUsersActivityAsync();

        // Assert
        Assert.True(saveActivityResult?.IsSuccess);
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
    }

    [Fact(Skip = "NotImplemented")]
    public async Task SetUndefinedActivityToAllUsersAsync_Once_AddUndefinedStateToAll()
    {
        throw new NotImplementedException();

        // Arrange
        //var activityLoggerService = GetActivityLogger(_userIdSet);
        //var users = await _usersRepository!.FindAllAsync();
        //var before = await _activityLogItemsRepository!.FindLastUsersActivityAsync();
        //
        //// Act
        //var setUndefinedActivityResult = await activityLoggerService.SetUndefinedActivityToAllUsersAsync();
        //var after = await _activityLogItemsRepository.FindLastUsersActivityAsync();
        //after = after.OrderBy(i => i.Id).TakeLast(_dbEntitiesAmount).ToList(); // Because InMemory doesn't support RawSql
        //
        //// Assert
        //setUndefinedActivityResult.Should().NotBeNull();
        //setUndefinedActivityResult.IsSuccess.Should().BeTrue();
        //after.Should().HaveSameCount(users);
        //after.Should().OnlyContain(i => i.IsOnline == null);
    }

    [Fact(Skip = "NotImplemented")]
    public async Task SetUndefinedActivityToAllUsersAsync_ManyTimes_AddUndefinedActivityOnlyOnce()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "NotImplemented")]
    public async Task SetUndefinedActivityToAllUsersAsync_EmptyActivityLog_SuccessfulWithWarning()
    {
        throw new NotImplementedException();
    }


    private IActivityLogger GetActivityLogger()
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(_dbEntitiesAmount);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.GetFullPath(Constants.VkActivityServiceAppSettingsPath))
            .Build();

        var vkIntegration = new VkIntegration(configuration[AppSettings.Vk.AccessToken], configuration[AppSettings.Vk.Version]);

        return new ActivityLogger(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.UsersRepository,
            vkIntegration,
            Mock.Of<ILogger<ActivityLogger>>(),
            Mock.Of<IDelayedLogger<ActivityLogger>>());
    }
}
