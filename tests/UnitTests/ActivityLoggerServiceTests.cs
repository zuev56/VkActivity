using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Models.VkApi;
using VkActivity.Service.Services;
using Xunit;

namespace UnitTests;

public class ActivityLoggerServiceTests
{
    private const int _dbEntitiesAmount = 1000;
    private const int _newUsersCount = 100;

    private readonly int[] _initialUserIds;
    private readonly string[] _initialUserStringIds;
    private readonly int[] _newUserIds;
    private readonly string[] _newUserStringIds;
    private readonly int _existingUsersAmountWhenAddNew = 10;
    private readonly int[] _newAndExistingUserIds;
    private readonly string[] _newAndExistingUserStringIds;


    public ActivityLoggerServiceTests()
    {
        _initialUserIds = Enumerable.Range(1, _dbEntitiesAmount).ToArray();
        _initialUserStringIds = _initialUserIds.Select(x => x.ToString()).ToArray();

        _newUserIds = Enumerable.Range(_dbEntitiesAmount + 1, _newUsersCount).ToArray();
        _newUserStringIds = _newUserIds.Select(x => x.ToString()).ToArray();

        _newAndExistingUserIds = _initialUserIds.Take(_existingUsersAmountWhenAddNew).Union(_newUserIds).ToArray();
        _newAndExistingUserStringIds = _newAndExistingUserIds.Select(x => x.ToString()).ToArray();
    }

    [Fact]
    public async Task SaveVkUsersActivityAsync_ReturnsSuccess()
    {
        // Arrange
        var activityLoggerService = GetActivityLoggerService();

        // Act
        var saveActivityResult = await activityLoggerService.SaveVkUsersActivityAsync();

        // Assert
        Assert.True(saveActivityResult?.IsSuccess);
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
    }

    [Fact]
    public async Task SaveVkUsersActivityAsync_VkIntegrationFailed_ReturnsError()
    {
        // Arrange
        var activityLoggerService = GetActivityLoggerService(vkIntergationWorks: false);

        // Act
        var saveActivityResult = await activityLoggerService.SaveVkUsersActivityAsync();

        // Assert
        Assert.False(saveActivityResult?.IsSuccess);
        Assert.NotEmpty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Error));
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Info));
    }

    [Fact]
    public async Task AddNewUsersAsync_OnlyNewUserIds_ReturnsSuccess()
    {
        // Arrange
        var activityLoggerService = GetActivityLoggerService();

        // Act
        var addUsersResult = await activityLoggerService.AddNewUsersAsync(_newUserStringIds);

        // Assert
        Assert.True(addUsersResult?.IsSuccess);
        Assert.False(addUsersResult?.HasWarnings);
        Assert.Empty(addUsersResult?.Messages);
        Assert.Equal(_newUsersCount, addUsersResult!.Value.Count);
    }

    [Fact]
    public async Task AddNewUsersAsync_WithExistingUserIds_ReturnsSuccessWithWarning()
    {
        // Arrange
        var activityLoggerService = GetActivityLoggerService();

        // Act
        var addUsersResult = await activityLoggerService.AddNewUsersAsync(_newAndExistingUserStringIds);

        // Assert
        Assert.True(addUsersResult?.IsSuccess);
        Assert.True(addUsersResult?.HasWarnings);
        Assert.Equal(_newUsersCount, addUsersResult!.Value.Count);
    }

    private ActivityLogger GetActivityLoggerService(bool vkIntergationWorks = true)
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(_dbEntitiesAmount);

        var vkIntegrationMock = new Mock<IVkIntegration>();
        if (vkIntergationWorks)
        {
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(_initialUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(_initialUserIds));
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(_newUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(_newUserIds));
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(_newAndExistingUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(_newAndExistingUserIds));

#warning GetUsersWithFullInfoAsync returns UsersWithActivityInfo
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(_initialUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(_initialUserIds));
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(_newUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(_newUserIds));
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(_newAndExistingUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(_newAndExistingUserIds));
        }
        else
        {
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();
        }

        return new ActivityLogger(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.VkUsersRepository,
            vkIntegrationMock.Object,
            Mock.Of<ILogger<ActivityLogger>>());
    }

    private List<VkApiUser> GetUsersWithActivity(int[] userIds)
    {
        var users = new List<VkApiUser>(userIds.Length);

        var sbUsersJsonArray = new StringBuilder("[");
        foreach (var id in userIds)
        {
            sbUsersJsonArray.Append(
               $@"{{
                    ""id"": {id},
                    ""first_name"": ""FirstName_{id}"",
                    ""last_name"": ""LastName_{id}"",
                    ""can_access_closed"": true,
                    ""is_closed"": false,
                    ""online"": {Random.Shared.Next(0, 1)},
                    ""last_seen"":{{
                        ""platform"": {Random.Shared.Next(1, 7)},
                        ""time"":1651338052
                    }}
                }},");
        }
        sbUsersJsonArray.Insert(sbUsersJsonArray.Length - 1, ']');

        using (var document = JsonDocument.Parse(sbUsersJsonArray.ToString().TrimEnd(',')))
        {
            foreach (var user in document.RootElement.EnumerateArray())
                users.Add(JsonSerializer.Deserialize<VkApiUser>(user)!);
        }

        return users;
    }
}
