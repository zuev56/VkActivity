using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VkActivity.Service;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Services;
using Xunit;

namespace IntegrationTests;

public class VkIntegrationTests
{
    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ReceiveScreenNames_ReturnsExpectedUsers()
    {
        // Arrange
        var vkIntegration = GetVkIntegration();
        var screenNames = new string[] { "durov", "zuev56" };

        // Act
        var users = await vkIntegration.GetUsersWithActivityInfoAsync(screenNames);

        // Assert
        Assert.NotNull(users);
        Assert.Equal(screenNames.Length, users.Count);
    }

    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ReceiveIds_ReturnsExpectedUsers()
    {
        // Arrange
        var vkIntegration = GetVkIntegration();
        var userIds = new string[] { "1", "8790237" };

        // Act
        var users = await vkIntegration.GetUsersWithActivityInfoAsync(userIds);

        // Assert
        Assert.NotNull(users);
        Assert.Equal(userIds.Length, users.Count);
    }

    [Fact]
    public async Task GetUsersWithActivityInfoAsync_ManyRequests_WorksCorrect()
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
            Assert.Equal(2, users.Count);
        }
        sw.Stop();

        Assert.True(sw.Elapsed > attempts * VkIntegration.ApiAccessMinInterval);
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
