using System.Linq;
using System.Threading.Tasks;
using UnitTests.Data;
using Xunit;

namespace UnitTests;

public class ActivityLoggerTests
{
    private const int _dbEntitiesAmount = 1000;
    private readonly UserIdSet _userIdSet = UserIdSet.Create(_dbEntitiesAmount);

    [Fact]
    public async Task SaveVkUsersActivityAsync_ReturnsSuccess()
    {
        // Arrange
        var activityLoggerService = StubFactory.GetActivityLogger(_userIdSet);

        // Act
        var saveActivityResult = await activityLoggerService.SaveUsersActivityAsync();

        // Assert
        Assert.True(saveActivityResult?.IsSuccess);
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
    }

    [Fact(Skip = "Unable to save default activity because EF Inmemory not works with RawSql")]
    public async Task SaveVkUsersActivityAsync_VkIntegrationFailed_ReturnsError()
    {
        // Arrange
        var activityLoggerService = StubFactory.GetActivityLogger(_userIdSet, vkIntergationWorks: false);

        // Act
        var saveActivityResult = await activityLoggerService.SaveUsersActivityAsync();

        // Assert
        Assert.False(saveActivityResult?.IsSuccess);
        Assert.NotEmpty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Error));
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
        Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Info));
    }
}
