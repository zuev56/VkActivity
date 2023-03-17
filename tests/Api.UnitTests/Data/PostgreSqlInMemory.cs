using Microsoft.EntityFrameworkCore;
using VkActivity.Data;
using VkActivity.Data.Repositories;

namespace Api.UnitTests.Data;

public sealed class PostgreSqlInMemory
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

    public void FillWithFakeData(int entitiesCount)
    {
        var users = StubFactory.CreateUsers(entitiesCount);
        var activityLogItems = StubFactory.CreateActivityLogItems(entitiesCount - 10);


        var t = Task.WhenAll(UsersRepository.SaveRangeAsync(users), ActivityLogItemsRepository.SaveRangeAsync(activityLogItems));
        t.Wait();
    }
}