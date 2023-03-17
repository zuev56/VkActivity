using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VkActivity.Common.Abstractions;
using VkActivity.Common.Services;
using Xunit;

namespace Worker.IntegrationTests;

[ExcludeFromCodeCoverage]
public sealed class VkIntegrationTests : TestBase
{
    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ByScreenNames_ReturnsExpectedUsers()
    {
        var screenNames = new [] { "durov", "zuev56" };
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();

        var users = await vkIntegration.GetUsersWithActivityInfoAsync(screenNames);

        Assert.NotNull(users);
        Assert.DoesNotContain(null, users);
        Assert.Equal(screenNames.Length, users.Count);
    }

    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ByIds_ReturnsExpectedUsers()
    {
        var userIds = new [] { "1", "8790237" };
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();

        var users = await vkIntegration.GetUsersWithActivityInfoAsync(userIds);

        Assert.NotNull(users);
        Assert.DoesNotContain(null, users);
        Assert.Equal(userIds.Length, users.Count);
    }

    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ManyRequests_ExecuteWithDelay()
    {
        var screenNames = new [] { "1", "8790237" };
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();
        var sw = new Stopwatch();
        var attempts = 10;

        sw.Start();
        for (var i = 0; i < attempts; i++)
        {
            var users = await vkIntegration.GetUsersWithActivityInfoAsync(screenNames);

            // Assert
            Assert.NotNull(users);
            Assert.DoesNotContain(null, users);
            Assert.Equal(2, users.Count);
        }
        sw.Stop();

        Assert.True(sw.Elapsed > attempts * VkIntegration.ApiAccessMinInterval);
    }

    [Fact]
    public async Task GetUsersWithFullInfoAsync_ReturnsUsersWithFullInfo()
    {
        var screenNames = new [] { "durov", "zuev56" };
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();

        var users = await vkIntegration.GetUsersWithFullInfoAsync(screenNames);

        Assert.NotNull(users);
        Assert.DoesNotContain(null, users);
        Assert.Contains(null, users.Select(u => u.LastSeen));
        Assert.Contains(0, users.Select(u => u.IsOnline));
        Assert.Equal(screenNames.Length, users.Count);
    }

    [Theory]
    [InlineData(8790237)]
    public async Task GetFriendIds_ReturnsFriendIdsArray(int userId)
    {
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();

        var friendIds = await vkIntegration.GetFriendIds(userId);

        friendIds.Should().NotBeNull();
        friendIds.Should().HaveCountGreaterThan(100);
        friendIds.Should().OnlyHaveUniqueItems();
    }
}