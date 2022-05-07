using System;
using System.Collections.Generic;
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
    internal static User CreateVkUser_v5_131(int userId = 0)
    {
        userId = PrepareId(userId);
        var firstName = $"TestVkUserFirstName_{userId}";
        var lastName = $"TestVkUserLastName_{userId}";

        return new User
        {
            Id = userId,
            FirstName = firstName,
            LastName = lastName,
            RawData = $@"{{
                ""blacklisted"": {Random.Shared.Next(0, 1)},
                ""blacklisted_by_me"": {Random.Shared.Next(0, 1)},
                ""can_access_closed"": true,
                ""can_be_invited_group"": false,
                ""can_post"": {Random.Shared.Next(0, 1)},
                ""can_see_all_posts"": {Random.Shared.Next(0, 1)},
                ""can_see_audio"": {Random.Shared.Next(0, 1)},
                ""can_send_friend_request"": {Random.Shared.Next(0, 1)},
                ""can_write_private_message"": {Random.Shared.Next(0, 1)},
                ""country"": {{
                  ""id"": 1,
                  ""title"": ""Россия""
                }},
                ""domain"": ""id{userId}"",
                ""first_name"": ""{firstName}"",
                ""followers_count"": {Random.Shared.Next(0, 150)},
                ""friend_status"": {Random.Shared.Next(0, 1)},
                ""has_mobile"": {Random.Shared.Next(0, 1)},
                ""has_photo"": {Random.Shared.Next(0, 1)},
                ""id"": {userId},
                ""is_closed"": false,
                ""is_friend"": {Random.Shared.Next(0, 1)},
                ""is_hidden_from_feed"": {Random.Shared.Next(0, 1)},
                ""last_name"": ""{lastName}"",
                ""last_seen"": {{
                  ""platform"": {Random.Shared.Next(1, 7)},
                  ""time"": {(DateTime.UtcNow - TimeSpan.FromMinutes(Random.Shared.Next(0, 10))).ToUnixEpoch()}
                }},
                ""nickname"": """",
                ""occupation"": {{
                  ""id"": 62296,
                  ""name"": ""КФ ПетрГУ"",
                  ""type"": ""university""
                }},
                ""online"": {Random.Shared.Next(0, 1)},
                ""photo_50"": ""https://sun9-87.userapi.com/s/v1/if1/825Nv7C4JARdESnx6PDnEbPWuvNBbi7tPe4oOuRlGco7xqacQvOXVKHjtwhDvc8-Hh5fIaJ2.jpg?size=50x50&quality=96&crop=0,299,1620,1620&ava=1"",
                ""screen_name"": ""id{userId}"",
                ""sex"": {Random.Shared.Next(1, 2)},
                ""site"": """",
                ""skype"": """",
                ""status"": """",
                ""verified"": {Random.Shared.Next(0, 1)}
            }}",
            InsertDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow
        };
    }

    private static int PrepareId(int id)
        => id != 0 ? id : Random.Shared.Next(1, 9999);

    internal static User[] CreateVkUsers(int amount)
    {
        var users = new User[amount];

        for (int i = 0; i < amount; i++)
            users[i] = CreateVkUser_v5_131(i + 1);

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

    internal static ActivityLogger GetActivityLoggerService(UserIdSet userIdSet, bool vkIntergationWorks = true)
    {
        var postgreSqlInMemory = new PostgreSqlInMemory();
        postgreSqlInMemory.FillWithFakeData(userIdSet.InitialUsersAmount);

        var vkIntegrationMock = new Mock<IVkIntegration>();
        if (vkIntergationWorks)
        {
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(userIdSet.InitialUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(userIdSet.InitialUserIds));
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(userIdSet.NewUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(userIdSet.NewUserIds));
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(userIdSet.NewAndExistingUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(userIdSet.NewAndExistingUserIds));

#warning GetUsersWithFullInfoAsync returns UsersWithActivityInfo
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(userIdSet.InitialUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(userIdSet.InitialUserIds));
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(userIdSet.NewUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(userIdSet.NewUserIds));
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(userIdSet.NewAndExistingUserStringIds))
                .ReturnsAsync(GetUsersWithActivity(userIdSet.NewAndExistingUserIds));
        }
        else
        {
            vkIntegrationMock.Setup(m => m.GetUsersWithActivityInfoAsync(It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();
            vkIntegrationMock.Setup(m => m.GetUsersWithFullInfoAsync(It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();
        }

        return new ActivityLogger(
            postgreSqlInMemory.ActivityLogItemsRepository,
            postgreSqlInMemory.VkUsersRepository,
            vkIntegrationMock.Object,
            Mock.Of<ILogger<ActivityLogger>>());
    }

    private static List<VkApiUser> GetUsersWithActivity(int[] userIds)
    {
        var users = new List<VkApiUser>(userIds.Length);

        var sbUsersJsonArray = new StringBuilder("[");
        foreach (var id in userIds)
        {
            sbUsersJsonArray.Append(
               $@"{{
                    ""id"": {id},
                    ""first_name"": ""FirstName_{id}"",
                    ""last_name"": ""LastName_{id}"",
                    ""can_access_closed"": true,
                    ""is_closed"": false,
                    ""online"": {Random.Shared.Next(0, 1)},
                    ""last_seen"":{{
                        ""platform"": {Random.Shared.Next(1, 7)},
                        ""time"":1651338052
                    }}
                }},");
        }
        sbUsersJsonArray.Insert(sbUsersJsonArray.Length - 1, ']');

        using (var document = JsonDocument.Parse(sbUsersJsonArray.ToString().TrimEnd(',')))
        {
            foreach (var user in document.RootElement.EnumerateArray())
                users.Add(JsonSerializer.Deserialize<VkApiUser>(user)!);
        }

        return users;
    }

}