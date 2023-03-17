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
public sealed class ActivityLoggerTests : TestBase
{
    private const int DbEntitiesAmount = 1000;

    public ActivityLoggerTests()
    {
        AddRealUsersAsync(DbEntitiesAmount).Wait();
    }

    [Fact]
    public async Task SaveVkUsersActivityAsync_Should_ReturnSuccess()
    {
        var activityLogger = ServiceProvider.GetRequiredService<IActivityLogger>();

        var saveActivityResult = await activityLogger.SaveUsersActivityAsync();

        saveActivityResult?.Successful.Should().BeTrue();
        //saveActivityResult?.Messages
        //    .Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning)
        //    .Should().BeEmpty();

        await Task.Delay(1000);
    }

    [Fact]
    public async Task SetUndefinedActivityToAllUsersAsync_Should_AddUndefinedStateToAll()
    {
        var activityLogger = ServiceProvider.GetRequiredService<IActivityLogger>();
        var usersRepository = ServiceProvider.GetRequiredService<IUsersRepository>();
        var activityLogsRepository = ServiceProvider.GetRequiredService<IActivityLogItemsRepository>();
        var users = await usersRepository!.FindAllAsync();
        var existingUsers = users.Where(u => u.Status == Status.Active).ToList();
        await activityLogger.SaveUsersActivityAsync();
        var activitiesBefore = await activityLogsRepository!.FindLastUsersActivityAsync();

        var setUndefinedActivityResult = await activityLogger.ChangeAllUserActivitiesToUndefinedAsync();
        var activitiesAfter = await activityLogsRepository.FindLastUsersActivityAsync();

        setUndefinedActivityResult.Should().NotBeNull();
        setUndefinedActivityResult.Successful.Should().BeTrue();
        activitiesAfter.Should().NotBeEquivalentTo(activitiesBefore);
        activitiesAfter.Should().HaveCountLessThanOrEqualTo(existingUsers.Count);
        activitiesAfter.Should().OnlyContain(i => i.IsOnline == null);

        await Task.Delay(1000);
    }

    [Fact]
    public async Task SetUndefinedActivityToAllUsersAsync_Should_AddUndefinedActivityOnlyOnce_When_InvokesManyTimes()
    {
        var activityLogger = ServiceProvider.GetRequiredService<IActivityLogger>();
        var activityLogsRepository = ServiceProvider.GetRequiredService<IActivityLogItemsRepository>();
        await activityLogger.SaveUsersActivityAsync();

        var setUndefinedActivityResult1 = await activityLogger.ChangeAllUserActivitiesToUndefinedAsync();
        var activitiesAfter1 = await activityLogsRepository.FindLastUsersActivityAsync();
        var setUndefinedActivityResult2 = await activityLogger.ChangeAllUserActivitiesToUndefinedAsync();
        var activitiesAfter2 = await activityLogsRepository.FindLastUsersActivityAsync();

        setUndefinedActivityResult1.Should().NotBeNull();
        setUndefinedActivityResult1.Successful.Should().BeTrue();
        setUndefinedActivityResult2.Should().NotBeNull();
        setUndefinedActivityResult2.Successful.Should().BeTrue();
        activitiesAfter1.Should().BeEquivalentTo(activitiesAfter2);

        await Task.Delay(1000);
    }

    [Fact]
    public async Task ChangeAllUserActivitiesToUndefinedAsync_ShouldDoNothing_When_EmptyActivityLog()
    {
        var activityLogger = ServiceProvider.GetRequiredService<IActivityLogger>();
        var activityLogsRepository = ServiceProvider.GetRequiredService<IActivityLogItemsRepository>();

        var setUndefinedActivityResult = await activityLogger.ChangeAllUserActivitiesToUndefinedAsync();
        var activitiesAfter = await activityLogsRepository.FindLastUsersActivityAsync();

        setUndefinedActivityResult.Should().NotBeNull();
        setUndefinedActivityResult.Successful.Should().BeFalse();
        activitiesAfter.Should().BeEmpty();

        await Task.Delay(1000);
    }
}