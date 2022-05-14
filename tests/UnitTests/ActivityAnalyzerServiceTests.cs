using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Services;
using Xunit;

namespace UnitTests;

public class ActivityAnalyzerServiceTests
{
    private const int _dbEntitiesAmount = 1000;

    [Fact]
    public async Task GetFullTimeUserStatisticsAsync_CorrectUserId_ReturnsNotNull()
    {
        // Arrange
        var activityAnalyzerService = GetActivityAnalyzerService();
        var userId = Random.Shared.Next(1, _dbEntitiesAmount);

        // Act
        var fullTimeUserActivity = await activityAnalyzerService.GetFullTimeActivityAsync(userId);

        // Assert
        Assert.NotNull(fullTimeUserActivity);
    }

    private IActivityAnalyzer GetActivityAnalyzerService()
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(_dbEntitiesAmount);

        return new ActivityAnalyzer(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.VkUsersRepository,
            Mock.Of<ILogger<ActivityAnalyzer>>());
    }
}
