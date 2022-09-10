using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;
using VkActivity.Worker.Abstractions;
using Xunit;

namespace Worker.IntegrationTests;

[ExcludeFromCodeCoverage]
public class ActivityLoggerTests : TestBase
{
    private const int _dbEntitiesAmount = 1000;

    public ActivityLoggerTests()
    {
        AddRealUsersAsync(_dbEntitiesAmount).Wait();
    }

    [Fact]
    public async Task SaveVkUsersActivityAsync_Should_ReturnSuccess()
    {
        // Arrange
        var activityLogger = ServiceProvider.GetRequiredService<IActivityLogger>();

        // Act
        var saveActivityResult = await activityLogger.SaveUsersActivityAsync();

        // Assert
        saveActivityResult?.IsSuccess.Should().BeTrue();
        saveActivityResult?.Messages
            .Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning)
            .Should().BeEmpty();

        await Task.Delay(1000);
    }

    [Fact]
    public async Task SetUndefinedActivityToAllUsersAsync_Should_AddUndefinedStateToAll()
    {
        // Arrange
        var activityLogger = ServiceProvider.GetRequiredService<IActivityLogger>();
        var usersRepository = ServiceProvider.GetRequiredService<IUsersRepository>();
        var activityLogsRepository = ServiceProvider.GetRequiredService<IActivityLogItemsRepository>();
        var users = await usersRepository!.FindAllAsync();
        var existingUsers = users.Where(u => u.Status == Status.Active).ToList();
        await activityLogger.SaveUsersActivityAsync();
        var activitiesBefore = await activityLogsRepository!.FindLastUsersActivityAsync();

        // Act
        var setUndefinedActivityResult = await activityLogger.ChangeAllUserActivitiesToUndefinedAsync();
        var activitiesAfter = await activityLogsRepository.FindLastUsersActivityAsync();

        // Assert
        setUndefinedActivityResult.Should().NotBeNull();
        setUndefinedActivityResult.IsSuccess.Should().BeTrue();
        activitiesAfter.Should().NotBeEquivalentTo(activitiesBefore);
        activitiesAfter.Should().HaveCountLessThanOrEqualTo(existingUsers.Count);
        activitiesAfter.Should().OnlyContain(i => i.IsOnline == null);

        await Task.Delay(1000);
    }

    [Fact]
    public async Task SetUndefinedActivityToAllUsersAsync_Should_AddUndefinedActivityOnlyOnce_When_InvokesManyTimes()
    {
        // Arrange
        var activityLogger = ServiceProvider.GetRequiredService<IActivityLogger>();
        var activityLogsRepository = ServiceProvider.GetRequiredService<IActivityLogItemsRepository>();
        await activityLogger.SaveUsersActivityAsync();

        // Act
        var setUndefinedActivityResult1 = await activityLogger.ChangeAllUserActivitiesToUndefinedAsync();
        var activitiesAfter1 = await activityLogsRepository.FindLastUsersActivityAsync();
        var setUndefinedActivityResult2 = await activityLogger.ChangeAllUserActivitiesToUndefinedAsync();
        var activitiesAfter2 = await activityLogsRepository.FindLastUsersActivityAsync();

        // Assert
        setUndefinedActivityResult1.Should().NotBeNull();
        setUndefinedActivityResult1.IsSuccess.Should().BeTrue();
        setUndefinedActivityResult2.Should().NotBeNull();
        setUndefinedActivityResult2.IsSuccess.Should().BeTrue();
        activitiesAfter1.Should().BeEquivalentTo(activitiesAfter2);

        await Task.Delay(1000);
    }

    [Fact]
    public async Task ChangeAllUserActivitiesToUndefinedAsync_ShouldDoNothing_When_EmptyActivityLog()
    {
        // Arrange
        var activityLogger = ServiceProvider.GetRequiredService<IActivityLogger>();
        var activityLogsRepository = ServiceProvider.GetRequiredService<IActivityLogItemsRepository>();

        // Act
        var setUndefinedActivityResult = await activityLogger.ChangeAllUserActivitiesToUndefinedAsync();
        var activitiesAfter = await activityLogsRepository.FindLastUsersActivityAsync();

        // Assert
        setUndefinedActivityResult.Should().NotBeNull();
        setUndefinedActivityResult.IsSuccess.Should().BeTrue();
        setUndefinedActivityResult.HasWarnings.Should().BeTrue();
        activitiesAfter.Should().BeEmpty();

        await Task.Delay(1000);
    }
}
