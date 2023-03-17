using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using VkActivity.Common.Abstractions;
using VkActivity.Common.Models.VkApi;
using VkActivity.Common.Services;
using VkActivity.Data.Models;
using Zs.Common.Extensions;

[assembly: InternalsVisibleTo("Worker.IntegrationTests")]

namespace Api.UnitTests.Data;

// Extract to Common project
internal static class StubFactory
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

        for (var i = 0; i < amount; i++)
        {
            users[i] = CreateUser(i + 1);
        }

        return users;
    }

    internal static ActivityLogItem CreateActivityLogItem(int itemId, int userId, int lastSeen, bool isOnline)
    {
        var platform = (Platform)Convert.ToInt32(Random.Shared.Next(0, 8));

        return new ActivityLogItem
        {
            Id = itemId,
            UserId = userId,
            LastSeen = lastSeen,
            IsOnline = isOnline,
            Platform = platform,
            InsertDate = DateTime.UtcNow
        };
    }

    internal static ActivityLogItem[] CreateActivityLogItems(int usersCount)
    {
        // TODO: Вообще, правильным было бы заполнение через реальный функционал IActivityLogger
        var activityLogItems = new ActivityLogItem[usersCount * 3];
        var shift = 1;

        DateTime lastSeen;
        for (var i = 1; i < usersCount + 1; i++)
        {
            lastSeen = DateTime.UtcNow - TimeSpan.FromDays(Random.Shared.Next(10, 365));
            activityLogItems[i - 1] = CreateActivityLogItem(
                itemId: i,
                userId: i,
                lastSeen: lastSeen.ToUnixEpoch(),
                isOnline: true
                );
        }

        shift += usersCount;
        for (var i = 1; i < usersCount + 1; i++)
        {
            lastSeen = DateTime.UtcNow - TimeSpan.FromHours(Random.Shared.Next(10, 200));
            activityLogItems[i - 2 + shift] = CreateActivityLogItem(
                itemId: i - 1 + shift,
                userId: i,
                lastSeen.ToUnixEpoch(),
                isOnline: false
                );
        }

        shift += usersCount;
        for (var i = 0; i < usersCount; i += 3)
        {
            lastSeen = DateTime.UtcNow - TimeSpan.FromMinutes(Random.Shared.Next(0, 500));
            activityLogItems[i - 2 + shift] = CreateActivityLogItem(
                itemId: i - 1 + shift,
                userId: i,
                lastSeen.ToUnixEpoch(),
                isOnline: true
                );
        }
        return activityLogItems.Where(i => i != null).ToArray();
    }

    public static Mock<IVkIntegration> CreateVkIntegrationMock(UserIdSet userIdSet, bool vkIntergationWorks)
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
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(userIdSet.ChangedExistingUserStringIds))
                .ReturnsAsync(GetApiUsersWithUpdates(userIdSet.ChangedExistingUserIds));
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
            postgreSqlInMemory.UsersRepository,
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
            {
                users.Add(JsonSerializer.Deserialize<VkApiUser>(user)!);
            }
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