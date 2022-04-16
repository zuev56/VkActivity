using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Data;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Models;
using VkActivity.Service.Services;
using Xunit;

namespace UnitTests
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

            var vkIntegrationMock = new Mock<IVkIntegration>();
            vkIntegrationMock.Setup(m => m.GetUsersActivity(It.IsAny<int[]>())).ReturnsAsync(GetVkApiResponseWithActivity());
            vkIntegrationMock.Setup(m => m.GetUsers(It.IsAny<int[]>())).ReturnsAsync(GetVkApiResponseWithActivity());


            return new ActivityLoggerService(
                postgreSqlInMemory.ActivityLogItemsRepository,
                postgreSqlInMemory.VkUsersRepository,
                vkIntegrationMock.Object,
                Mock.Of<ILogger<ActivityLoggerService>>());
        }

        VkApiResponse GetVkApiResponseWithActivity() 
            => new VkApiResponse { Users = GetUsers(_dbEntitiesAmount).ToList() };

        private IEnumerable<VkApiUser> GetUsers(int dbEntitiesAmount)
        {
            for (int i = 0; i < dbEntitiesAmount; i++)
                yield return new VkApiUser { Id = i };
        }
    }
}