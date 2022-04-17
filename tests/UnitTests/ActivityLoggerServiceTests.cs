using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Models;
using VkActivity.Service.Services;
using Xunit;

namespace UnitTests;

public class ActivityLoggerServiceTests
{
    private readonly int _dbEntitiesAmount = 100;


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
    public async Task AddNewUsersAsync_NonExistentUserId_ReturnsSuccess()
    {
        // Arrange
        var activityLoggerService = GetActivityLoggerService();
        int[] newUserIds = GetTestVkUserIds();

        // Act
        var addUsersResult = await activityLoggerService.AddNewUsersAsync(newUserIds);

        // Assert
        Assert.True(addUsersResult?.IsSuccess);
        Assert.False(addUsersResult?.HasWarnings);
        Assert.Empty(addUsersResult?.Messages);
    }

    [Fact]
    public async Task AddNewUsersAsync_ExistingUserId_ReturnsSuccessWithWarning()
    {
        // Arrange
        var activityLoggerService = GetActivityLoggerService();
        var newUserIds = GetTestVkUserIds().Union(new[] { 1, 2, 3 }).ToArray();

        // Act
        var addUsersResult = await activityLoggerService.AddNewUsersAsync(newUserIds);
        
        // Assert
        Assert.True(addUsersResult?.IsSuccess);
        Assert.True(addUsersResult?.HasWarnings);
        Assert.Equal(GetTestVkUserIds().Length, addUsersResult!.Value.Count);
    }

    private ActivityLoggerService GetActivityLoggerService(bool vkIntergationWorks = true)
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(_dbEntitiesAmount);

        var vkIntegrationMock = new Mock<IVkIntegration>();
        if (vkIntergationWorks)
        {
            vkIntegrationMock.Setup(m => m.GetUsersActivityAsync(It.IsAny<int[]>())).ReturnsAsync(GetUsers());
            vkIntegrationMock.Setup(m => m.GetUsersAsync(It.IsAny<int[]>())).ReturnsAsync(GetUsers());
        }
        else
        {
            vkIntegrationMock.Setup(m => m.GetUsersActivityAsync(It.IsAny<int[]>())).Throws<InvalidOperationException>();
            vkIntegrationMock.Setup(m => m.GetUsersAsync(It.IsAny<int[]>())).Throws<InvalidOperationException>();
        }

        return new ActivityLoggerService(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.VkUsersRepository,
            vkIntegrationMock.Object,
            Mock.Of<ILogger<ActivityLoggerService>>());
    }

    private List<VkApiUser> GetUsers()
    {
        var testVkUserIds = GetTestVkUserIds();
        var users = new List<VkApiUser>(testVkUserIds.Length);
        
        foreach (var id in testVkUserIds)
            users.Add(new VkApiUser { Id = id });

        return users;
    }

    private int[] GetTestVkUserIds()
    {
        return new[]
        {
            _dbEntitiesAmount + 1,
            _dbEntitiesAmount + 2,
            _dbEntitiesAmount + 3,
            _dbEntitiesAmount + 456789,
            _dbEntitiesAmount + 12345678
        };
    }
}
