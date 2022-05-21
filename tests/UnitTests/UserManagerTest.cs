using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using UnitTests.Data;
using Xunit;

namespace UnitTests;

public class UserManagerTest
{
    private const int _dbEntitiesAmount = 1000;
    private const int _sublistAmountDivider = 10;
    private readonly int _newUsersCount = _dbEntitiesAmount / _sublistAmountDivider;
    private readonly UserIdSet _userIdSet = UserIdSet.Create(_dbEntitiesAmount, _sublistAmountDivider);


    [Fact]
    public async Task AddNewUsersAsync_AddAll_When_NewUserIds()
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
    public async Task AddNewUsersAsync_AddOnlyNew_When_NewAndExistingUserIds()
    {
        // Arrange
        var userManager = StubFactory.GetUserManager(_userIdSet);

        // Act
        var addUsersResult = await userManager.AddUsersAsync(_userIdSet.NewAndExistingUserStringIds);

        // Assert
        addUsersResult.IsSuccess.Should().BeTrue();
        addUsersResult.HasWarnings.Should().BeTrue();
        addUsersResult.Value.Should().HaveCount(_newUsersCount);
    }

    [Fact]
    public async Task AddNewUsersAsync_Fail_When_VkIntegrationNotWorks()
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
    public async Task UpdateUsersAsync_Successful_When_ExistingUsersChanged()
    {
        // Arrange
        var userManager = StubFactory.GetUserManager(_userIdSet);

        // Act
        var updateUsersResult = await userManager.UpdateUsersAsync(_userIdSet.ChangedExistingUserIds);

        // Assert
        updateUsersResult.IsSuccess.Should().BeTrue();
        updateUsersResult.HasWarnings.Should().BeFalse();
        updateUsersResult?.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateUsersAsync_SuccessfulWithWarnings_When_ExistingAndUnknownUsers()
    {
        // Arrange
        var userManager = StubFactory.GetUserManager(_userIdSet);
        var userIdsToUpdate = _userIdSet.ChangedExistingUserIds.Union(_userIdSet.NewUserIds).ToArray();

        // Act
        var updateUsersResult = await userManager.UpdateUsersAsync(userIdsToUpdate);

        // Assert
        updateUsersResult.IsSuccess.Should().BeTrue();
        updateUsersResult.HasWarnings.Should().BeTrue();
        updateUsersResult?.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateUsersAsync_Fail_When_UserIdsArrayIsNull()
    {
        // Arrange
        var userManager = StubFactory.GetUserManager(_userIdSet);

        // Act
        var updateUsersResult = await userManager.UpdateUsersAsync(null!);

        // Assert
        updateUsersResult.IsSuccess.Should().BeFalse();
        updateUsersResult.HasWarnings.Should().BeFalse();
        updateUsersResult?.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateUsersAsync_Fail_When_UserIdsArrayIsEmpty()
    {
        // Arrange
        var userManager = StubFactory.GetUserManager(_userIdSet);

        // Act
        var updateUsersResult = await userManager.UpdateUsersAsync(new int[0]);

        // Assert
        updateUsersResult.IsSuccess.Should().BeFalse();
        updateUsersResult.HasWarnings.Should().BeFalse();
        updateUsersResult?.Messages.Should().NotBeEmpty();
    }

}
