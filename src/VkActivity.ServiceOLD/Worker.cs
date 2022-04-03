using Microsoft.Extensions.Configuration;
using Zs.Common.Abstractions;

namespace VkActivity.Service;

public class Worker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IActivityLoggerService _activityLoggerService;
    private readonly ILogger<Worker> _logger;

    private int _idleStepsCount = 10;

    public Worker(
        IConfiguration configuration,
        IActivityLoggerService activityService,
        ILogger<Worker> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _activityLoggerService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        _logger = logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_configuration.GetValue<int>("Home:Vk:ActivityLogIntervalSec"));

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await SaveVkUsersActivityAsync().ConfigureAwait(false);
            await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
        }
    }


    /// <summary>Activity data collection</summary>
    private async Task SaveVkUsersActivityAsync()
    {
        if (_idleStepsCount > 0)
        {
            _idleStepsCount--;
            return;
        }

        var result = await _activityLoggerService.SaveVkUsersActivityAsync();

        if (!result.IsSuccess)
        {
            string logMessage = result.Messages.Count == 1
                ? result.Messages[0].Text
                : string.Join(Environment.NewLine, result.Messages.Select((m, i) => $"{i + 1}. {m.Text}"));

            _logger?.LogError(logMessage);
        }
    }

}
