using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VkActivity.Service;
using VkActivity.Service.Models;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;

namespace Home.Bot.Services
{
    // TODO: В хранимке vk.sf_cmd_get_not_active_users выводить точное количество времени отсутствия

    // TODO: Надо сделать нормальное добавление пользователей:
    //          1. идентификаторы пользователей в БД равны идентификаторам в ВК
    //          2. При первом запуске в БД добавляются отсутствующие идентификаторы из конфига, если такие есть
    //          3. Далее работаем только с пользователями из БД, т.к.возможно их добавление "на лету"

    internal class UserWatcher : IUserWatcher
    {
        private readonly IConfiguration _configuration;
        private readonly IActivityLoggerService _activityLoggerService;
        private readonly IDbClient _dbClient;
        private readonly ILogger<UserWatcher> _logger;

        private IJob _userActivityLoggerJob;
        private bool _isFirstStep = true;

        public IReadOnlyCollection<IJobBase> Jobs { get; private init; }

        public UserWatcher(
            IConfiguration configuration,
            IActivityLoggerService activityService,
            IDbClient dbClient,
            ILogger<UserWatcher> logger = null)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _activityLoggerService = activityService ?? throw new ArgumentNullException(nameof(activityService));
                _dbClient = dbClient ?? throw new ArgumentNullException(nameof(dbClient));
                _logger = logger;

                Jobs = CreateJobs();
            }
            catch (Exception ex)
            {
                var tiex = new TypeInitializationException(typeof(UserWatcher).FullName, ex);
               _logger?.LogError(tiex, $"{nameof(UserWatcher)} initialization error");
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _activityLoggerService.AddNewUsersAsync(_configuration.GetSection("Home:Vk:UserIds").Get<int[]>());
                _logger?.LogInformation($"{nameof(UserWatcher)} started");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"{nameof(UserWatcher)} starting error");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation($"{nameof(UserWatcher)} stopped");
            return Task.CompletedTask;
        }

        private List<IJobBase> CreateJobs()
        {
            _userActivityLoggerJob = new ProgramJob(
                period: TimeSpan.FromSeconds(_configuration.GetSection("Home:Vk:ActivityLogIntervalSec").Get<int>()),
                method: SaveVkUsersActivityAsync,
                description: "logUserStatus", 
                logger: _logger);

            var notActiveUsersInformerJob = new SqlJob(
                period: TimeSpan.FromHours(1),
                resultType: QueryResultType.String,
                sqlQuery: $"select vk.sf_cmd_get_not_active_users('{string.Join(',', _configuration.GetSection("Home:Vk:TrackedUserIds").Get<int[]>())}', {_configuration.GetSection("Home:Vk:AlarmAfterInactiveHours").Get<int>()})",
                dbClient: _dbClient,
                startUtcDate: DateTime.UtcNow + TimeSpan.FromSeconds(5),
                description: Constants.USER_WATCHER_INFORMING_JOB_NAME);

            return new List<IJobBase>()
            {
                _userActivityLoggerJob,
                notActiveUsersInformerJob
            };
        }

        /// <summary>Activity data collection</summary>
        private async Task SaveVkUsersActivityAsync()
        {
            if (_isFirstStep)
            {
                _isFirstStep = false;
                _userActivityLoggerJob.IdleStepsCount = 10;
            }

            var result = await _activityLoggerService.SaveVkUsersActivityAsync();

            if (_userActivityLoggerJob.IdleStepsCount > 0)
                _userActivityLoggerJob.IdleStepsCount = 0;

            if (!result.IsSuccess)
            {
                string logMessage = result.Messages.Count == 1
                    ? result.Messages[0].Text
                    : string.Join(Environment.NewLine, result.Messages.Select((m, i) => $"{i + 1}. {m.Text}"));

                _logger?.LogError(logMessage);
            }
        }
    }
}
