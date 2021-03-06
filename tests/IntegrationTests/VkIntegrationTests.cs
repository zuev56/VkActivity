using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using VkActivity.Worker;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Services;
using Xunit;

namespace IntegrationTests;

public class VkIntegrationTests
{
    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ByScreenNames_ReturnsExpectedUsers()
    {
        // Arrange
        var vkIntegration = GetVkIntegration();
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
        var vkIntegration = GetVkIntegration();
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
        var vkIntegration = GetVkIntegration();
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
        var vkIntegration = GetVkIntegration();
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
        var vkIntegration = GetVkIntegration();
        var userId = 8790237; //Надо перебирать пользователей с открытыми друзьями

        // Act
        var friendIds = await vkIntegration.GetFriendIds(userId);

        // Assert
        friendIds.Should().NotBeNull();
        friendIds.Should().HaveCountGreaterThan(100);
        friendIds.Should().OnlyHaveUniqueItems();
    }

    private IVkIntegration GetVkIntegration()
    {
        var configuration = new ConfigurationBuilder()
           .AddJsonFile(Path.GetFullPath(Constants.VkActivityServiceAppSettingsPath))
           .Build();

        var token = configuration[AppSettings.Vk.AccessToken];
        var version = configuration[AppSettings.Vk.Version];

        return new VkIntegration(token, version);
    }
}
