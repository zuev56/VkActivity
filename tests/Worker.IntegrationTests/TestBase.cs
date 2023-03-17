using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using VkActivity.Common;
using VkActivity.Common.Abstractions;
using VkActivity.Common.Services;
using VkActivity.Data;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Repositories;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Services;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Logging.DelayedLogger;

namespace Worker.IntegrationTests;

[ExcludeFromCodeCoverage]
public abstract class TestBase : IDisposable
{
    private static readonly string DbName = $"VkActivityTEST_{Guid.NewGuid()}";
    protected readonly ServiceProvider ServiceProvider;

    protected TestBase()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            //.AddUserSecrets<TestBase>()
            .Build();

        ServiceProvider = CreateServiceProvider(configuration);

        InitializeDataBase();
    }

    private static ServiceProvider CreateServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddDbContext<VkActivityContext>(options =>
        {
            var connectionString = configuration["ConnectionStrings:Default"];
            var csb = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = DbName
            };
            connectionString = csb.ToString();
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IDbContextFactory<VkActivityContext>, VkActivityContextFactory>();
        services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();

        services.AddConnectionAnalyzer();
        services.AddVkIntegration(configuration);

        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton(_ => Mock.Of<ILogger<ActivityLogger>>());
        services.AddSingleton(_ => Mock.Of<IDelayedLogger<ActivityLogger>>());
        services.AddSingleton(_ => Mock.Of<ILogger<UserManager>>());

        services.AddScoped<IUserManager, UserManager>();
        services.AddScoped<IActivityLogger, ActivityLogger>();

        services.AddVkIntegration(configuration);

        return services.BuildServiceProvider();
    }

    private void InitializeDataBase()
    {
        using var scope = ServiceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<VkActivityContext>();

        context.Database.EnsureCreated();
    }

    protected async Task AddRealUsersAsync(int amount)
    {
        var usersManager = ServiceProvider.GetRequiredService<IUserManager>();
        var userIds = new string[amount];

        for (var i = 0; i < amount; i++)
        {
            var id = Random.Shared.Next(1, 40000000).ToString();
            if (userIds.Contains(id))
            {
                i--;
                continue;
            }
            userIds[i] = id;
        }

        var addUsersResult = await usersManager.AddUsersAsync(userIds);
        addUsersResult.EnsureSuccess();
    }

    public void Dispose()
    {
        using var scope = ServiceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var dbContext = scopedServices.GetRequiredService<VkActivityContext>();
        dbContext.Database.EnsureDeleted();
    }
}