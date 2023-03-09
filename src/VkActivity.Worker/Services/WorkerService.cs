using VkActivity.Common;
using VkActivity.Common.Abstractions;
using VkActivity.Data.Abstractions;
using VkActivity.Worker.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Logging.DelayedLogger;
using Zs.Common.Services.Scheduling;
using static VkActivity.Worker.Models.Constants;

namespace VkActivity.Worker.Services;

internal sealed class WorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IScheduler _scheduler;
    private readonly IConnectionAnalyzer _connectionAnalyzer;
    private readonly IConfiguration _configuration;
    private readonly IDelayedLogger<WorkerService> _delayedLogger;
    private readonly ILogger<WorkerService> _logger;
    private DateTime _disconnectionTime = DateTime.UtcNow;

    private Job _userActivityLoggerJob = null!;
    private bool _isFirstStep = true;


    public WorkerService(
        IServiceScopeFactory serviceScopeFactory,
        IScheduler scheduler,
        IConnectionAnalyzer connectionAnalyzer,
        IConfiguration configuration,
        IDelayedLogger<WorkerService> delayedLogger,
        ILogger<WorkerService> logger)
    {
        try
        {
            _scopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _connectionAnalyzer = connectionAnalyzer ?? throw new ArgumentNullException(nameof(connectionAnalyzer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _delayedLogger = delayedLogger ?? throw new ArgumentNullException(nameof(delayedLogger));
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _connectionAnalyzer.ConnectionStatusChanged += СonnectionAnalyser_StatusChanged;
            _connectionAnalyzer.Start(2.Seconds(), 3.Seconds());

            _scheduler.Start(3.Seconds(), 1.Seconds());

            _logger.LogInformation($"{nameof(WorkerService)} started");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(WorkerService)} starting error");
            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _connectionAnalyzer.ConnectionStatusChanged -= СonnectionAnalyser_StatusChanged;
        _connectionAnalyzer.Stop();

        _scheduler.Stop();

        _logger.LogInformation($"{nameof(WorkerService)} stopped");
        return Task.CompletedTask;
    }

    private void СonnectionAnalyser_StatusChanged(ConnectionStatus status)
    {
        _disconnectionTime = status == ConnectionStatus.Ok
            ? default
            : DateTime.UtcNow;
    }

    private List<JobBase> CreateJobs()
    {
        _userActivityLoggerJob = new ProgramJob(
            period: TimeSpan.FromSeconds(_configuration.GetSection(AppSettings.Vk.ActivityLogIntervalSec).Get<int>()),
            method: SaveVkUsersActivityAsync,
            description: nameof(_userActivityLoggerJob),
            logger: _logger);

        var userDataUpdaterJob = new ProgramJob(
            period: TimeSpan.FromHours(_configuration.GetSection(AppSettings.Vk.UsersDataUpdateIntervalHours).Get<int>()),
            startUtcDate: DateTime.UtcNow.Date.AddDays(1),
            method: UpdateUsersDataAsync,
            description: "userDataUpdaterJob",
            logger: _logger);

        return new List<JobBase>()
        {
            _userActivityLoggerJob,
            userDataUpdaterJob
        };
    }

    /// <summary>Activity data collection</summary>
    private async Task SaveVkUsersActivityAsync()
    {
        if (_disconnectionTime != default)
        {
            await SetUndefinedActivityToAllUsers().ConfigureAwait(false);

            _delayedLogger.LogError(NoInernetConnection);
            return;
        }

        if (_isFirstStep)
        {
            _isFirstStep = false;
            _userActivityLoggerJob.IdleStepsCount = 1;
            await AddUsersFullInfoAsync().ConfigureAwait(false);
            return;
        }

        if (_userActivityLoggerJob.IdleStepsCount > 0)
            _userActivityLoggerJob.IdleStepsCount = 0;

        var result = await SaveUsersActivityAsync().ConfigureAwait(false);
        if (!result.Successful)
        {
            string logMessage = result.Fault!.Code;

            _logger.LogErrorIfNeed(logMessage);
        }

        //_logger.LogTraceIfNeed(result.JoinMessages());
    }

    private async Task<Result> SaveUsersActivityAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var activityLoggerService = scope.ServiceProvider.GetRequiredService<IActivityLogger>();
            return await activityLoggerService.SaveUsersActivityAsync().ConfigureAwait(false);
        }
    }

    private async Task<Result> SetUndefinedActivityToAllUsers()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var activityLoggerService = scope.ServiceProvider.GetRequiredService<IActivityLogger>();
            return await activityLoggerService.ChangeAllUserActivitiesToUndefinedAsync().ConfigureAwait(false);
        }
    }
    private async Task AddUsersFullInfoAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
            var initialUserIds = _configuration.GetSection(AppSettings.Vk.InitialUserIds).Get<string[]>();
            await userManager.AddUsersAsync(initialUserIds ?? Array.Empty<string>()).ConfigureAwait(false);
        }
    }

    private async Task UpdateUsersDataAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var usersRepo = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            var userIds = await usersRepo.FindAllIdsAsync().ConfigureAwait(false);

            var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
            var updateResult = await userManager.UpdateUsersAsync(userIds);

            if (!updateResult.Successful)
                _logger.LogError(string.Join(Environment.NewLine, updateResult.Fault!.Code), new { UserIds = userIds });
        }
    }
}