using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VkActivity.Data;
using VkActivity.Data.Repositories;

namespace UnitTests.Data;

public class PostgreSqlInMemory
{
    public ActivityLogItemsRepository ActivityLogItemsRepository { get; }
    public UsersRepository VkUsersRepository { get; }

    public PostgreSqlInMemory()
    {
        var dbContextFactory = GetPostgreSqlBotContextFactory();

        ActivityLogItemsRepository = new ActivityLogItemsRepository(dbContextFactory);
        VkUsersRepository = new UsersRepository(dbContextFactory);
    }

    private VkActivityContextFactory GetPostgreSqlBotContextFactory()
    {
        var dbName = $"PostgreSQLInMemoryDB_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<VkActivityContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new VkActivityContextFactory(options);
    }

    public void FillWithFakeData(int entitiesCount)
    {
        var users = StubFactory.CreateUsers(entitiesCount);
        var activityLogItems = StubFactory.CreateActivityLogItems(entitiesCount - 10);


        Task.WaitAll(new Task[]
        {
            VkUsersRepository.SaveRangeAsync(users),
            ActivityLogItemsRepository.SaveRangeAsync(activityLogItems)
        });
    }
}