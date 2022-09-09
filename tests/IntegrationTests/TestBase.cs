using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VkActivity.Data;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Repositories;
using VkActivity.Worker;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Helpers;
using VkActivity.Worker.Services;
using static Worker.IntegrationTests.Constants;

namespace Worker.IntegrationTests
{
    [ExcludeFromCodeCoverage]
    public abstract class TestBase : IDisposable
    {
        private readonly string _testDbName = $"VkActivityTEST_{Guid.NewGuid()}";
        protected readonly ServiceProvider ServiceProvider;

        protected TestBase()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.GetFullPath(VkActivityServiceAppSettingsPath))
                .AddUserSecrets<TestBase>()
                .Build();

            ServiceProvider = CreateServiceProvider(configuration, _testDbName);

            InitializeDataBase();
        }

        private static ServiceProvider CreateServiceProvider(IConfiguration configuration, string dbName)
        {
            var services = new ServiceCollection();
            services.AddDbContext<VkActivityContext>(options =>
            {
                var connectionString = $"Host=localhost;Persist Security Info=True;"
                    + $"Port={configuration[DbPortSecretsKey]};"
                    + $"Database={dbName};"
                    + $"Username={configuration[DbUserSecretsKey]};"
                    + $"Password={configuration[DbPasswordSecretsKey]};";
                options.UseNpgsql(connectionString);
            });

            services.AddScoped<IDbContextFactory<VkActivityContext>, VkActivityContextFactory>();
            services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();

            services.AddConnectionAnalyzer(configuration);
            services.AddVkIntegration(configuration);

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(sp => Mock.Of<ILogger<ActivityLogger>>());
            services.AddSingleton(sp => Mock.Of<IDelayedLogger<ActivityLogger>>());
            services.AddSingleton(sp => Mock.Of<ILogger<UserManager>>());

            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IActivityLogger, ActivityLogger>();

            return services.BuildServiceProvider();
        }

        private void InitializeDataBase()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<VkActivityContext>();

                db.Database.EnsureCreated();
            }
        }

        protected async Task AddRealUsersAsync(int amount)
        {
            var usersManager = ServiceProvider.GetRequiredService<IUserManager>();

            var userIds = new string[amount];

            for (int i = 0; i < amount; i++)
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
            using (var scope = ServiceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var dbContext = scopedServices.GetRequiredService<VkActivityContext>();

                dbContext.Database.EnsureDeleted();
            }

        }
    }
}