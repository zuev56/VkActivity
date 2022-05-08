using System.Linq;
using System.Threading.Tasks;
using UnitTests.Data;
using Xunit;

namespace UnitTests;

public class UserManagerTests
{
    private const int _dbEntitiesAmount = 1000;
    private const int _sublistAmountDivider = 10;
    private readonly int _newUsersCount = _dbEntitiesAmount / _sublistAmountDivider;
    private readonly UserIdSet _userIdSet = UserIdSet.Create(_dbEntitiesAmount, _sublistAmountDivider);


    [Fact]
    public async Task AddNewUsersAsync_OnlyNewUserIds_ReturnsSuccess()
    {
        // Arrange
        var userManager = StubFactory.GetUserManager(_userIdSet);

        // Act
        var addUsersResult = await userManager.AddUsersAsync(_userIdSet.NewUserStringIds);

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
        var userManager = StubFactory.GetUserManager(_userIdSet);

        // Act
        var addUsersResult = await userManager.AddUsersAsync(_userIdSet.NewAndExistingUserStringIds);

        // Assert
        Assert.True(addUsersResult?.IsSuccess);
        Assert.True(addUsersResult?.HasWarnings);
        Assert.Equal(_newUsersCount, addUsersResult!.Value.Count);
    }

    [Fact]
    public async Task AddNewUsersAsync_VkIntegrationNotWorks_ReturnsError()
    {
        // Arrange
        var userManager = StubFactory.GetUserManager(_userIdSet, vkIntergationWorks: false);

        // Act
        var addUsersResult = await userManager.AddUsersAsync(_userIdSet.NewUserStringIds);

        // Assert
        Assert.False(addUsersResult?.IsSuccess);
        Assert.NotEmpty(addUsersResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Error));
        Assert.Empty(addUsersResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
        Assert.Empty(addUsersResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Info));
    }


    [Fact]
    public async Task UpdateUsersAsync__ReturnsSuccess()
    {
        // Arrange
        var userManager = StubFactory.GetUserManager(_userIdSet);
        var usersToUpdate = StubFactory.GetUsersToUpdate(_userIdSet.ChangedUserIds);

        // Act
        var updateUsersResult = await userManager.UpdateUsersAsync(_userIdSet.ChangedUserIds);

        // Assert
        Assert.True(updateUsersResult?.IsSuccess);
        Assert.False(updateUsersResult?.HasWarnings);
        Assert.Empty(updateUsersResult?.Messages);
    }

}
