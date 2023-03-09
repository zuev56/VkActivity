using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VkActivity.Data;
using VkActivity.Data.Repositories;

namespace UnitTests.Data;

public class PostgreSqlInMemory
{
    public ActivityLogItemsRepository ActivityLogItemsRepository { get; }
    public UsersRepository UsersRepository { get; }

    public PostgreSqlInMemory()
    {
        var dbContextFactory = GetPostgreSqlBotContextFactory();

        ActivityLogItemsRepository = new ActivityLogItemsRepository(dbContextFactory);
        UsersRepository = new UsersRepository(dbContextFactory);
    }

    private VkActivityContextFactory GetPostgreSqlBotContextFactory()
    {
        var dbName = $"PostgreSQLInMemoryDB_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<VkActivityContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new VkActivityContextFactory(options);
    }

    public async Task FillWithFakeDataAsync(int entitiesCount)
    {
        var users = StubFactory.CreateUsers(entitiesCount);
        var activityLogItems = StubFactory.CreateActivityLogItems(entitiesCount - 10);


        //Task.WaitAll(UsersRepository.SaveRangeAsync(users), ActivityLogItemsRepository.SaveRangeAsync(activityLogItems));
        var t1 = await UsersRepository.SaveRangeAsync(users);
        var t2 = await ActivityLogItemsRepository.SaveRangeAsync(activityLogItems);

        //var t = await Task.WhenAll(, ActivityLogItemsRepository.SaveRangeAsync(activityLogItems));
    }
}