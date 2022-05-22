using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Models;
using VkActivity.Worker.Services;
using Xunit;

namespace UnitTests;

public class ActivityAnalyzerTests
{
    private const int _dbEntitiesAmount = 1000;
    private static readonly DateTime _utcNow = DateTime.UtcNow;
    public static readonly object[][] WrongDateIntervals =
    {
        new object[] { _utcNow, _utcNow },
        new object[] { _utcNow, _utcNow - TimeSpan.FromMilliseconds(1) }
    };


    [Fact]
    public async Task GetFullTimeActivityAsync_Successful_When_CorrectUserId()
    {
        for (int i = 0; i < _dbEntitiesAmount / 10; i++)
        {
            // Arrange
            var activityAnalyzer = GetActivityAnalyzer();
            var userId = Random.Shared.Next(1, _dbEntitiesAmount);

            // Act
            var result = await activityAnalyzer.GetFullTimeActivityAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.UserId.Should().Be(userId);
        }
    }

    [Fact(Skip = "NotImplemented")]
    public Task GetFullTimeActivityAsync_SuccessfulWithCorrectActivityTime()
    {
        throw new NotImplementedException();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task GetFullTimeActivityAsync_Fail_When_UnknownUserId(int userId)
    {
        // Arrange
        var activityAnalyzer = GetActivityAnalyzer();

        // Act
        var result = await activityAnalyzer.GetFullTimeActivityAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Messages.Should().OnlyContain(m => m.Text == Note.UserNotFound(userId));
    }

    [Fact]
    public async Task GetFullTimeActivityAsync_HasWarning_When_NoDataForUser()
    {
        // Arrange
        var userWithoutActivityDataId = _dbEntitiesAmount - 1;
        var activityAnalyzer = GetActivityAnalyzer();

        // Act
        var result = await activityAnalyzer.GetFullTimeActivityAsync(userWithoutActivityDataId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Messages.Should().OnlyContain(m => m.Text == Note.ActivityForUserNotFound(userWithoutActivityDataId));
        result.Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task GetUserStatisticsForPeriodAsync_Successful_When_CorrectParameters()
    {
        for (int i = 0; i < _dbEntitiesAmount / 10; i++)
        {
            // Arrange
            var userId = i + 1;
            var fromDate = _utcNow - TimeSpan.FromHours(Random.Shared.Next(10, 100));
            var toDate = _utcNow - TimeSpan.FromHours(Random.Shared.Next(0, 9));
            var activityAnalyzer = GetActivityAnalyzer();

            // Act
            var result = await activityAnalyzer.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task GetUserStatisticsForPeriodAsync_Fail_When_UnknownUserId(int userId)
    {
        // Arrange
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.MaxValue;
        var activityAnalyzer = GetActivityAnalyzer();

        // Act
        var result = await activityAnalyzer.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Messages.Should().OnlyContain(m => m.Text == Note.UserNotFound(userId));
        result.Messages.Should().ContainSingle();
    }

    [Theory]
    [MemberData(nameof(WrongDateIntervals))]
    public async Task GetUserStatisticsForPeriodAsync_Fail_When_ToDateNotMoreThanFromDate(
        DateTime fromDate, DateTime toDate)
    {
        // Arrange
        var userId = Random.Shared.Next(1, _dbEntitiesAmount);
        var activityAnalyzer = GetActivityAnalyzer();

        // Act
        var result = await activityAnalyzer.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Messages.Should().OnlyContain(m => m.Text == Note.EndDateIsNotMoreThanStartDate);
        result.Messages.Should().ContainSingle();
    }

    public static readonly object[][] _correctParametersForGetUsersWithActivityAsync =
    {
        new object[] { "", DateTime.MinValue, DateTime.MaxValue },
        new object[] { null!, DateTime.MinValue, DateTime.MaxValue },
        new object[] { "Te", _utcNow - TimeSpan.FromHours(3), _utcNow },
        new object[] { "!@$^", _utcNow - TimeSpan.FromDays(30), _utcNow + TimeSpan.FromDays(30) },
        new object[] { "Er", new DateTime(2017, 2, 1), new DateTime(2018, 2, 28) },
        new object[] { "1", new DateTime(2095, 2, 1), new DateTime(2187, 2, 28) }
    };

    [Theory]
    [MemberData(nameof(_correctParametersForGetUsersWithActivityAsync))]
    public async Task GetUsersWithActivityAsync_Successful_When_CorrectParameters(
        string filterText, DateTime fromDate, DateTime toDate)
    {
        // Arrange
        var activityAnalyzer = GetActivityAnalyzer();

        // Act
        var result = await activityAnalyzer.GetUsersWithActivityAsync(filterText, fromDate, toDate);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(WrongDateIntervals))]
    public async Task GetUsersWithActivityAsync_Fail_When_ToDateNotMoreThanFromDate(
        DateTime fromDate, DateTime toDate)
    {
        // Arrange
        var filterText = string.Empty;
        var activityAnalyzer = GetActivityAnalyzer();

        // Act
        var result = await activityAnalyzer.GetUsersWithActivityAsync(filterText, fromDate, toDate);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Messages.Should().OnlyContain(m => m.Text == Note.EndDateIsNotMoreThanStartDate);
        result.Messages.Should().ContainSingle();
    }

    private static IActivityAnalyzer GetActivityAnalyzer()
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(_dbEntitiesAmount);

        return new ActivityAnalyzer(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.VkUsersRepository,
            Mock.Of<ILogger<ActivityAnalyzer>>());
    }
}
