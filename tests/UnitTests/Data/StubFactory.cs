using System;
using VkActivity.Data.Models;

namespace UnitTests.Data;

internal class StubFactory
{
    internal static User CreateVkUser(int userId = 0)
    {
        userId = PrepareId(userId);

        return new User
        {
            Id = userId,
            FirstName = $"TestVkUserFirstName_{userId}",
            LastName = $"TestVkUserLastName_{userId}",
            RawData = @"{
                            ""can_access_closed"": true,
                            ""first_name"": ""TestVkUserFirstName"",
                            ""id"": 123456,
                            ""is_closed"": false,
                            ""last_name"": ""TestVkUserLastName"",
                            ""online"": 0
                        }",
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
            users[i] = CreateVkUser(i + 1);

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
}
