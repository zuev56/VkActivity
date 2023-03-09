using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VkActivity.Common;
using VkActivity.Common.Abstractions;
using VkActivity.Common.Services;
using Xunit;

namespace Worker.IntegrationTests;

[ExcludeFromCodeCoverage]
public class VkIntegrationTests : TestBase
{
    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ByScreenNames_ReturnsExpectedUsers()
    {
        // Arrange
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();
        var screenNames = new string[] { "durov", "zuev56" };

        // Act
        var users = await vkIntegration.GetUsersWithActivityInfoAsync(screenNames);

        // Assert
        Assert.NotNull(users);
        Assert.DoesNotContain(null, users);
        Assert.Equal(screenNames.Length, users.Count);
    }

    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ByIds_ReturnsExpectedUsers()
    {
        // Arrange
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();
        var userIds = new string[] { "1", "8790237" };

        // Act
        var users = await vkIntegration.GetUsersWithActivityInfoAsync(userIds);

        // Assert
        Assert.NotNull(users);
        Assert.DoesNotContain(null, users);
        Assert.Equal(userIds.Length, users.Count);
    }

    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ManyRequests_ExecuteWithDelay()
    {
        // Arrange
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();
        var screenNames = new string[] { "1", "8790237" };
        var sw = new Stopwatch();
        var attempts = 10;

        // Act
        sw.Start();
        for (int i = 0; i < attempts; i++)
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
        // Arrange
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();
        var screenNames = new string[] { "durov", "zuev56" };

        // Act
        var users = await vkIntegration.GetUsersWithFullInfoAsync(screenNames);

        // Assert
        Assert.NotNull(users);
        Assert.DoesNotContain(null, users);
        Assert.Contains(null, users.Select(u => u.LastSeen));
        Assert.Contains(0, users.Select(u => u.IsOnline));
        Assert.Equal(screenNames.Length, users.Count);
    }

    [Fact]
    public async Task GetFriendIds_ReturnsFriendIdsArray()
    {
        // Arrange
        var vkIntegration = ServiceProvider.GetRequiredService<IVkIntegration>();
        var userId = 8790237; //Надо перебирать пользователей с открытыми друзьями

        // Act
        var friendIds = await vkIntegration.GetFriendIds(userId);

        // Assert
        friendIds.Should().NotBeNull();
        friendIds.Should().HaveCountGreaterThan(100);
        friendIds.Should().OnlyHaveUniqueItems();
    }
}