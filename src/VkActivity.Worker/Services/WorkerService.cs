using VkActivity.Data.Abstractions;
using VkActivity.Worker.Abstractions;
using Zs.Common.Abstractions;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;

namespace VkActivity.Worker.Services;

// TODO: В хранимке vk.sf_cmd_get_not_active_users выводить точное количество времени отсутствия
internal sealed class WorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IScheduler _scheduler;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WorkerService> _logger;

    private IJob? _userActivityLoggerJob;
    private bool _isFirstStep = true;


    public WorkerService(
        IServiceScopeFactory serviceScopeFactory,
        IScheduler scheduler,
        IConfiguration configuration,
        ILogger<WorkerService> logger)
    {
        try
        {
            _scopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _scheduler.Jobs.AddRange(CreateJobs());
        }
        catch (Exception ex)
        {
            var tiex = new TypeInitializationException(typeof(WorkerService).FullName, ex);
            _logger?.LogError(tiex, $"{nameof(WorkerService)} initialization error");
            throw;
        }
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetService<IUserManager>();
                var initialUserIds = scope.ServiceProvider.GetService<IConfiguration>()?.GetSection(AppSettings.Vk.InitialUserIds).Get<string[]>();
                await userManager!.AddUsersAsync(initialUserIds ?? Array.Empty<string>()).ConfigureAwait(false);
            }

            _scheduler.Start(3000, 1000);

            _logger.LogInformation($"{nameof(WorkerService)} started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(WorkerService)} starting error");
            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(WorkerService)} stopped");
        return Task.CompletedTask;
    }

    private List<IJobBase> CreateJobs()
    {
        _userActivityLoggerJob = new ProgramJob(
            period: TimeSpan.FromSeconds(_configuration.GetSection(AppSettings.Vk.ActivityLogIntervalSec).Get<int>()),
            method: SaveVkUsersActivityAsync,
            logger: _logger);

        var userDataUpdaterJob = new ProgramJob(
            period: TimeSpan.FromHours(_configuration.GetSection(AppSettings.Vk.UsersDataUpdateIntervalHours).Get<int>()),
            startUtcDate: DateTime.UtcNow.Date.AddDays(1),
            method: UpdateUsersDataAsync,
            logger: _logger);

        return new List<IJobBase>()
        {
            _userActivityLoggerJob,
            userDataUpdaterJob
        };
    }

    /// <summary>Activity data collection</summary>
    private async Task SaveVkUsersActivityAsync()
    {
        if (_isFirstStep)
        {
            _isFirstStep = false;
            _userActivityLoggerJob!.IdleStepsCount = 10;
        }

        IOperationResult result;
        using (var scope = _scopeFactory.CreateScope())
        {
            var activityLoggerService = scope.ServiceProvider.GetService<IActivityLogger>();
            result = await activityLoggerService!.SaveVkUsersActivityAsync().ConfigureAwait(false);
        }

        if (_userActivityLoggerJob!.IdleStepsCount > 0)
            _userActivityLoggerJob.IdleStepsCount = 0;

        if (!result.IsSuccess)
        {
            string logMessage = result.Messages.Count == 1
                ? result.Messages[0].Text
                : string.Join(Environment.NewLine, result.Messages.Select((m, i) => $"{i + 1}. {m.Text}"));

            _logger.LogError(logMessage);
        }
    }

    private async Task UpdateUsersDataAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var usersRepo = scope.ServiceProvider.GetService<IUsersRepository>();
            var userIds = await usersRepo!.FindAllIdsAsync().ConfigureAwait(false);

            var userManager = scope.ServiceProvider.GetService<IUserManager>()!;
            var updateResult = await userManager.UpdateUsersAsync(userIds);

            if (!updateResult.IsSuccess)
                _logger.LogError(string.Join(Environment.NewLine, updateResult.Messages), new { UserIds = userIds });
        }
    }


}
