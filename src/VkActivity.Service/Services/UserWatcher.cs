using VkActivity.Service.Abstractions;
using Zs.Common.Abstractions;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;

namespace VkActivity.Service.Services;

// TODO: В хранимке vk.sf_cmd_get_not_active_users выводить точное количество времени отсутствия
internal sealed class UserWatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IScheduler _scheduler;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserWatcher> _logger;

    private IJob? _userActivityLoggerJob;
    private bool _isFirstStep = true;


    public UserWatcher(
        IServiceScopeFactory serviceScopeFactory,
        IScheduler scheduler,
        IConfiguration configuration,
        ILogger<UserWatcher> logger)
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
            var tiex = new TypeInitializationException(typeof(UserWatcher).FullName, ex);
            _logger?.LogError(tiex, $"{nameof(UserWatcher)} initialization error");
            throw;
        }
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var activityLoggerService = scope.ServiceProvider.GetService<IActivityLoggerService>();
                var userIds = scope.ServiceProvider.GetService<IConfiguration>()?.GetSection("Vk:InitialUserIds").Get<int[]>();
                await activityLoggerService!.AddNewUsersAsync(userIds ?? Array.Empty<int>()).ConfigureAwait(false);
            }

            _scheduler.Start(3000, 1000);

            _logger.LogInformation($"{nameof(UserWatcher)} started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(UserWatcher)} starting error");
            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(UserWatcher)} stopped");
        return Task.CompletedTask;
    }

    private List<IJobBase> CreateJobs()
    {
        _userActivityLoggerJob = new ProgramJob(
            period: TimeSpan.FromMilliseconds(_configuration.GetSection("Vk:ActivityLogIntervalMs").Get<int>()),
            method: SaveVkUsersActivityAsync,
            description: "logUserStatus",
            logger: _logger);

        return new List<IJobBase>()
        {
            _userActivityLoggerJob
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
            var activityLoggerService = scope.ServiceProvider.GetService<IActivityLoggerService>();
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
}
