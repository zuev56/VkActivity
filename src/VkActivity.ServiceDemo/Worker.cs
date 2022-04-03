namespace VkActivity.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public static string ApiCommand { get; set; } = "No command";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker is running at {Time}, command: {Command}", DateTimeOffset.Now, ApiCommand);
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
