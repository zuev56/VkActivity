using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using VkActivity.Data.Models;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Models.VkApi;
using VkActivity.Service.Services;
using Zs.Common.Extensions;

namespace UnitTests.Data;

internal class StubFactory
{
    private static string CreateFirstName(int id) => $"TestVkUserFirstName_{id}";
    private static string CreateLastName(int id) => $"TestVkUserLastName_{id}";

    internal static User CreateUser(int userId = 0)
    {
        var firstName = CreateFirstName(userId);
        var lastName = CreateLastName(userId);
        userId = PrepareId(userId);

        var json = GetApiUserFullInfoJson_v5_131(userId);

        return new User
        {
            Id = userId,
            FirstName = firstName,
            LastName = lastName,
            RawData = json,
            InsertDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow
        };
    }

    private static string GetApiUserFullInfoJson_v5_131(int userId)
    {
        var firstName = CreateFirstName(userId);
        var lastName = CreateLastName(userId);
        return $@"{{
                ""id"": {userId},
                ""first_name"": ""{firstName}"",
                ""last_name"": ""{lastName}"",
                ""can_access_closed"": true,
                ""is_closed"": false,
                ""sex"": {Random.Shared.Next(1, 2)},
                ""screen_name"": ""id{userId}"",
                ""photo_50"": ""https://vk.com/images/camera_50.png"",
                ""verified"": {Random.Shared.Next(0, 1)},
                ""nickname"": """",
                ""domain"": ""id{userId}"",
                ""country"": {{
                  ""id"": 1,
                  ""title"": ""Россия""
                }},
                ""has_mobile"": {Random.Shared.Next(0, 1)},
                ""has_photo"": {Random.Shared.Next(0, 1)},
                ""skype"": """",
                ""site"": """",
                ""occupation"": {{
                  ""id"": 62296,
                  ""name"": ""КФ ПетрГУ"",
                  ""type"": ""university""
                }}
            }}";
    }

    private static string GetApiUserActivityInfoJson_v5_131(int userId)
    {
        var firstName = CreateFirstName(userId);
        var lastName = CreateLastName(userId);
        return $@"{{
                    ""id"": {userId},
                    ""first_name"": ""{firstName}"",
                    ""last_name"": ""{lastName}"",
                    ""can_access_closed"": true,
                    ""is_closed"": false,
                    ""online"": {Random.Shared.Next(0, 1)},
                    ""last_seen"":{{
                        ""platform"": {Random.Shared.Next(1, 7)},
                        ""time"": {(DateTime.UtcNow - TimeSpan.FromMinutes(Random.Shared.Next(0, 10))).ToUnixEpoch()}
                    }}
                }}";
    }

    private static int PrepareId(int id)
        => id != 0 ? id : Random.Shared.Next(1, 9999);

    internal static User[] CreateUsers(int amount)
    {
        var users = new User[amount];

        for (int i = 0; i < amount; i++)
            users[i] = CreateUser(i + 1);

        return users;
    }

    internal static ActivityLogItem CreateActivityLogItem(int itemId = 0, int userId = 0)
    {
        itemId = PrepareId(itemId);

        var isOnline = Convert.ToBoolean(Random.Shared.Next(0, 1));
        var platform = (Platform)Convert.ToInt32(Random.Shared.Next(0, 7));

        return new ActivityLogItem
        {
            Id = itemId,
            UserId = userId,
            IsOnline = isOnline,
            Platform = platform,
            InsertDate = DateTime.UtcNow
        };
    }

    internal static ActivityLogItem[] CreateActivityLogItems(int amount)
    {
        var activityLogItems = new ActivityLogItem[amount];

        for (int i = 0; i < amount; i++)
            activityLogItems[i] = CreateActivityLogItem(i + 1, i + 1);

        return activityLogItems;
    }

    internal static IActivityLogger GetActivityLogger(UserIdSet userIdSet, bool vkIntergationWorks = true)
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(userIdSet.InitialUsersAmount);

        var vkIntegrationMock = CreateVkIntegrationMock(userIdSet, vkIntergationWorks);

        return new ActivityLogger(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.VkUsersRepository,
            vkIntegrationMock.Object,
            Mock.Of<ILogger<ActivityLogger>>());
    }

    private static Mock<IVkIntegration> CreateVkIntegrationMock(UserIdSet userIdSet, bool vkIntergationWorks)
    {
        var vkIntegrationMock = new Mock<IVkIntegration>();
        if (vkIntergationWorks)
        {
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(userIdSet.InitialUserStringIds))
                .ReturnsAsync(GetApiUsers(userIdSet.InitialUserIds, id => GetApiUserActivityInfoJson_v5_131(id)));
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(userIdSet.NewUserStringIds))
                .ReturnsAsync(GetApiUsers(userIdSet.NewUserIds, id => GetApiUserActivityInfoJson_v5_131(id)));
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(userIdSet.NewAndExistingUserStringIds))
                .ReturnsAsync(GetApiUsers(userIdSet.NewAndExistingUserIds, id => GetApiUserActivityInfoJson_v5_131(id)));

#warning GetUsersWithFullInfoAsync returns UsersWithActivityInfo
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(userIdSet.InitialUserStringIds))
                .ReturnsAsync(GetApiUsers(userIdSet.InitialUserIds, id => GetApiUserFullInfoJson_v5_131(id)));
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(userIdSet.NewUserStringIds))
                .ReturnsAsync(GetApiUsers(userIdSet.NewUserIds, id => GetApiUserFullInfoJson_v5_131(id)));
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(userIdSet.NewAndExistingUserStringIds))
                .ReturnsAsync(GetApiUsers(userIdSet.NewAndExistingUserIds, id => GetApiUserFullInfoJson_v5_131(id)));
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(userIdSet.ChangedUserStringIds))
                .ReturnsAsync(GetApiUsersWithUpdates(userIdSet.ChangedUserIds));
        }
        else
        {
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();
        }

        return vkIntegrationMock;
    }

    internal static IUserManager GetUserManager(UserIdSet userIdSet, bool vkIntergationWorks = true)
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(userIdSet.InitialUsersAmount);

        var vkIntegrationMock = CreateVkIntegrationMock(userIdSet, vkIntergationWorks);

        return new UserManager(
            postgreSqlInMemory.VkUsersRepository,
            vkIntegrationMock.Object,
            Mock.Of<ILogger<UserManager>>());

    }

    private static List<VkApiUser> GetApiUsers(int[] userIds, Func<int, string> getApiUserJson)
    {
        var users = new List<VkApiUser>(userIds.Length);

        var sbUsersJsonArray = new StringBuilder("[");
        foreach (var id in userIds)
        {
            var vkUserJson = getApiUserJson.Invoke(id);
            sbUsersJsonArray.Append(vkUserJson).Append(',');
        }
        sbUsersJsonArray.Insert(sbUsersJsonArray.Length - 1, ']');

        using (var document = JsonDocument.Parse(sbUsersJsonArray.ToString().TrimEnd(',')))
        {
            foreach (var user in document.RootElement.EnumerateArray())
                users.Add(JsonSerializer.Deserialize<VkApiUser>(user)!);
        }

        return users;
    }

    private static List<VkApiUser> GetApiUsersWithUpdates(int[] userIds)
    {
        var changedUsers = new List<VkApiUser>(userIds.Length);
        foreach (var id in userIds)
        {
            var userFullInfoJson = GetApiUserFullInfoJson_v5_131(id);
            var user = JsonSerializer.Deserialize<VkApiUser>(userFullInfoJson)!;
            var changedUser = GetChangedUser(user);
            changedUsers.Add(changedUser);
        }

        return changedUsers;
    }

    private static VkApiUser GetChangedUser(VkApiUser user)
    {
        var changeType = Random.Shared.Next(1, 5);

        switch (changeType)
        {
            case 1:
                return new VkApiUser
                {
                    Id = user.Id,
                    FirstName = user.FirstName + "_changed",
                    LastName = user.LastName,
                    RawData = user.RawData
                };
            case 2:
                return new VkApiUser
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName + "_changed",
                    RawData = user.RawData
                };
            default:
                var newRawData = user.RawData!.ToDictionary(i => i.Key, i => i.Value.Clone());
                var updatedValue = user.RawData!["screen_name"] + "_changed";
                newRawData["screen_name"] = JsonSerializer.SerializeToElement(updatedValue);

                return new VkApiUser
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RawData = newRawData
                };
        }
    }
}