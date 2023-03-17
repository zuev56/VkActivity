using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Worker.UnitTests.Data;
using Xunit;

namespace Worker.UnitTests;

public sealed class UserManagerTest
{
    private const int DbEntitiesAmount = 1000;
    private const int SublistAmountDivider = 10;
    private readonly int _newUsersCount = DbEntitiesAmount / SublistAmountDivider;
    private readonly UserIdSet _userIdSet = UserIdSet.Create(DbEntitiesAmount, SublistAmountDivider);


    [Fact]
    public async Task AddNewUsersAsync_AddAll_When_NewUserIds()
    {
        var userManager = StubFactory.GetUserManager(_userIdSet);

        var addUsersResult = await userManager.AddUsersAsync(_userIdSet.NewUserStringIds);

        Assert.True(addUsersResult.Successful);
        //Assert.False(addUsersResult?.HasWarnings);
        //Assert.Empty(addUsersResult?.Messages);
        Assert.Equal(_newUsersCount, addUsersResult.Value.Count);
    }

    [Fact]
    public async Task AddNewUsersAsync_AddOnlyNew_When_NewAndExistingUserIds()
    {
        var userManager = StubFactory.GetUserManager(_userIdSet);

        var addUsersResult = await userManager.AddUsersAsync(_userIdSet.NewAndExistingUserStringIds);

        addUsersResult.Successful.Should().BeTrue();
        //addUsersResult.HasWarnings.Should().BeTrue();
        addUsersResult.Value.Should().HaveCount(_newUsersCount);
    }

    [Fact]
    public async Task AddNewUsersAsync_Fail_When_VkIntegrationNotWorks()
    {
        var userManager = StubFactory.GetUserManager(_userIdSet, vkIntergationWorks: false);

        var addUsersResult = await userManager.AddUsersAsync(_userIdSet.NewUserStringIds);

        Assert.False(addUsersResult.Successful);
        //Assert.NotEmpty(addUsersResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Error));
        //Assert.Empty(addUsersResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
        //Assert.Empty(addUsersResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Info));
    }

    [Fact]
    public async Task UpdateUsersAsync_Successful_When_ExistingUsersChanged()
    {
        var userManager = StubFactory.GetUserManager(_userIdSet);

        var updateUsersResult = await userManager.UpdateUsersAsync(_userIdSet.ChangedExistingUserIds);

        updateUsersResult.Successful.Should().BeTrue();
        //updateUsersResult.HasWarnings.Should().BeFalse();
        //updateUsersResult?.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateUsersAsync_SuccessfulWithWarnings_When_ExistingAndUnknownUsers()
    {
        var userManager = StubFactory.GetUserManager(_userIdSet);
        var userIdsToUpdate = _userIdSet.ChangedExistingUserIds.Union(_userIdSet.NewUserIds).ToArray();

        var updateUsersResult = await userManager.UpdateUsersAsync(userIdsToUpdate);

        updateUsersResult.Successful.Should().BeTrue();
        //updateUsersResult.HasWarnings.Should().BeTrue();
        //updateUsersResult?.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateUsersAsync_Fail_When_UserIdsArrayIsNull()
    {
        var userManager = StubFactory.GetUserManager(_userIdSet);

        var updateUsersResult = await userManager.UpdateUsersAsync(null!);

        updateUsersResult.Successful.Should().BeFalse();
        //updateUsersResult.HasWarnings.Should().BeFalse();
        //updateUsersResult?.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateUsersAsync_Fail_When_UserIdsArrayIsEmpty()
    {
        var userManager = StubFactory.GetUserManager(_userIdSet);

        var updateUsersResult = await userManager.UpdateUsersAsync(Array.Empty<int>());

        updateUsersResult.Successful.Should().BeFalse();
        //updateUsersResult.HasWarnings.Should().BeFalse();
        //updateUsersResult?.Messages.Should().NotBeEmpty();
    }
}