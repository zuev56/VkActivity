using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VkActivity.Common;
using VkActivity.Common.Abstractions;
using VkActivity.Common.Services;
using VkActivity.Data;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Repositories;
using VkActivity.Worker;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Services;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Logging.DelayedLogger;
using Zs.Common.Services.Scheduler;

[assembly: InternalsVisibleTo("Worker.UnitTests")]
[assembly: InternalsVisibleTo("Worker.IntegrationTests")]


IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices(ConfigureServices)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(host.Services.GetService<IConfiguration>())
    .CreateLogger();

Log.Warning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
    Process.GetCurrentProcess().MainModule?.ModuleName, Environment.MachineName,
    Environment.OSVersion, Environment.UserName, Environment.ProcessId);

await host.RunAsync();


void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddDbContext<VkActivityContext>(options =>
        options.UseNpgsql(context.Configuration.GetConnectionString(AppSettings.ConnectionStrings.Default)));

    services.AddScoped<IDbContextFactory<VkActivityContext>, VkActivityContextFactory>();

    services.AddConnectionAnalyzer(context.Configuration);
    services.AddVkIntegration(context.Configuration);
    services.AddSingleton<IScheduler, Scheduler>();
    // TODO: Create Factory!
    services.AddSingleton<IDelayedLogger<ActivityLogger>, DelayedLogger<ActivityLogger>>();
    services.AddSingleton<IDelayedLogger<WorkerService>, DelayedLogger<WorkerService>>();

    services.AddScoped<IUserManager, UserManager>();
    services.AddScoped<IActivityLogger, ActivityLogger>();

    services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
    services.AddScoped<IUsersRepository, UsersRepository>();

    services.AddHostedService<WorkerService>();
}