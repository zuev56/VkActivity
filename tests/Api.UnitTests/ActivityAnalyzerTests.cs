using Api.UnitTests.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VkActivity.Api.Abstractions;
using VkActivity.Api.Services;
using Xunit;

namespace Api.UnitTests;

public sealed class ActivityAnalyzerTests
{
    private const int DbEntitiesAmount = 1000;
    private static readonly DateTime TmpMinLogDate = new(2022, 09, 18);
    private static readonly DateTime UtcNow = DateTime.UtcNow;
    public static readonly object[][] WrongDateIntervals =
    {
        new object[] { UtcNow, UtcNow },
        new object[] { UtcNow, UtcNow - TimeSpan.FromMilliseconds(1) }
    };

    [Obsolete]
    [Fact]
    public async Task GetFullTimeActivityAsync_Successful_When_CorrectUserId()
    {
        for (var i = 0; i < DbEntitiesAmount / 10; i++)
        {
            var activityAnalyzer = GetActivityAnalyzer();
            var userId = Random.Shared.Next(1, DbEntitiesAmount);

            var result = await activityAnalyzer.GetUserStatisticsForPeriodAsync(userId, TmpMinLogDate, DateTime.UtcNow);

            result.Should().NotBeNull();
            result.Successful.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.UserId.Should().Be(userId);
        }
    }

    [Fact(Skip = "NotImplemented")]
    public Task GetFullTimeActivityAsync_SuccessfulWithCorrectActivityTime()
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task GetFullTimeActivityAsync_Fail_When_UnknownUserId(int userId)
    {
        var activityAnalyzer = GetActivityAnalyzer();

        var result = await activityAnalyzer.GetUserStatisticsForPeriodAsync(userId, TmpMinLogDate, DateTime.UtcNow);

        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.Value.Should().BeNull();
        //result.Messages.Should().OnlyContain(m => m.Text == Constants.UserNotFound(userId));
    }

    [Obsolete]
    [Fact]
    public async Task GetFullTimeActivityAsync_HasWarning_When_NoDataForUser()
    {
        var userWithoutActivityDataId = DbEntitiesAmount - 1;
        var activityAnalyzer = GetActivityAnalyzer();

        var result = await activityAnalyzer.GetUserStatisticsForPeriodAsync(userWithoutActivityDataId, TmpMinLogDate, DateTime.UtcNow);

        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Value.Should().NotBeNull();
        //result.Messages.Should().OnlyContain(m => m.Text == Constants.ActivityForUserNotFound(userWithoutActivityDataId));
        //result.Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task GetUserStatisticsForPeriodAsync_Successful_When_CorrectParameters()
    {
        for (var i = 0; i < DbEntitiesAmount / 10; i++)
        {
            var userId = i + 1;
            var fromDate = UtcNow - TimeSpan.FromHours(Random.Shared.Next(10, 100));
            var toDate = UtcNow - TimeSpan.FromHours(Random.Shared.Next(0, 9));
            var activityAnalyzer = GetActivityAnalyzer();

            var result = await activityAnalyzer.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);

            result.Should().NotBeNull();
            result.Successful.Should().BeTrue();
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
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.MaxValue;
        var activityAnalyzer = GetActivityAnalyzer();

        var result = await activityAnalyzer.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);

        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.Value.Should().BeNull();
        //result.Messages.Should().OnlyContain(m => m.Text == Constants.UserNotFound(userId));
        //result.Messages.Should().ContainSingle();
    }

    [Theory]
    [MemberData(nameof(WrongDateIntervals))]
    public async Task GetUserStatisticsForPeriodAsync_Fail_When_ToDateNotMoreThanFromDate(
        DateTime fromDate, DateTime toDate)
    {
        var userId = Random.Shared.Next(1, DbEntitiesAmount);
        var activityAnalyzer = GetActivityAnalyzer();

        var result = await activityAnalyzer.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);

        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.Value.Should().BeNull();
        //result.Messages.Should().OnlyContain(m => m.Text == Constants.EndDateIsNotMoreThanStartDate);
        //result.Messages.Should().ContainSingle();
    }

    public static readonly object[][] CorrectParametersForGetUsersWithActivityAsync =
    {
        new object[] { "", DateTime.MinValue, DateTime.MaxValue },
        new object[] { null!, DateTime.MinValue, DateTime.MaxValue },
        new object[] { "Te", UtcNow - TimeSpan.FromHours(3), UtcNow },
        new object[] { "!@$^", UtcNow - TimeSpan.FromDays(30), UtcNow + TimeSpan.FromDays(30) },
        new object[] { "Er", new DateTime(2017, 2, 1), new DateTime(2018, 2, 28) },
        new object[] { "1", new DateTime(2095, 2, 1), new DateTime(2187, 2, 28) }
    };

    [Theory]
    [MemberData(nameof(CorrectParametersForGetUsersWithActivityAsync))]
    public async Task GetUsersWithActivityAsync_Successful_When_CorrectParameters(
        string filterText, DateTime fromDate, DateTime toDate)
    {
        var activityAnalyzer = GetActivityAnalyzer();

        var result = await activityAnalyzer.GetUsersWithActivityAsync(fromDate, toDate, filterText);

        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(WrongDateIntervals))]
    public async Task GetUsersWithActivityAsync_Fail_When_ToDateNotMoreThanFromDate(
        DateTime fromDate, DateTime toDate)
    {
        var filterText = string.Empty;
        var activityAnalyzer = GetActivityAnalyzer();

        var result = await activityAnalyzer.GetUsersWithActivityAsync(fromDate, toDate, filterText);

        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.Value.Should().BeNull();
        //result.Messages.Should().OnlyContain(m => m.Text == Constants.EndDateIsNotMoreThanStartDate);
        //result.Messages.Should().ContainSingle();
    }

    private static IActivityAnalyzer GetActivityAnalyzer()
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(DbEntitiesAmount);

        return new ActivityAnalyzer(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.UsersRepository,
            Mock.Of<ILogger<ActivityAnalyzer>>());
    }
}