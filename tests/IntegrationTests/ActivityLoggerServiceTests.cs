using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Services;
using Xunit;

namespace IntegrationTests
{
    public class ActivityLoggerServiceTests
    {
        private readonly int _dbEntitiesAmount = 1000;


        [Fact]
        public async Task SaveVkUsersActivityAsync_InternetConnectionEstablished_ReturnsSuccess()
        {
            // Arrange
            var activityLoggerService = GetActivityLoggerService();

            // Act
            var saveActivityResult = await activityLoggerService.SaveVkUsersActivityAsync();

            // Assert
            Assert.True(saveActivityResult?.IsSuccess);
            Assert.Empty(saveActivityResult?.Messages.Where(m => m.Type == Zs.Common.Enums.InfoMessageType.Warning));
        }

        private IActivityLoggerService GetActivityLoggerService()
        {
            var postgreSqlInMemory = new PostgreSqlInMemory();
            postgreSqlInMemory.FillWithFakeData(_dbEntitiesAmount);
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(System.IO.Path.GetFullPath(@"..\..\..\..\..\src\VkActivity.Service\appsettings.Development.json"))
                .Build();

            return new ActivityLoggerService(
                configuration,
                postgreSqlInMemory.ActivityLogItemsRepository,
                postgreSqlInMemory.VkUsersRepository,
                Mock.Of<ILogger<ActivityLoggerService>>());
        }

    }
}