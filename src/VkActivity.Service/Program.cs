using System.Diagnostics;
using Home.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VkActivity.Data;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;
using VkActivity.Service;
using Zs.Common.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Logging.Seq;
using Zs.Common.Services.Scheduler;
using Zs.Common.Services.Shell;

Log.Warning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
    Process.GetCurrentProcess().MainModule?.ModuleName,
    Environment.MachineName,
    Environment.OSVersion,
    Environment.UserName,
    Environment.ProcessId);


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) => builder.AddConfiguration(CreateConfiguration(args)))
    .UseSerilog()
    .ConfigureServices(ConfigureServices)
    .Build();

await host.RunAsync();



IConfiguration CreateConfiguration(string[] args)
{
    if (!File.Exists(ProgramUtilites.MainConfigurationPath))
        throw new AppsettingsNotFoundException();

    var configuration = new ConfigurationManager();
    configuration.AddJsonFile(ProgramUtilites.MainConfigurationPath, optional: false, reloadOnChange: true);

    foreach (var arg in args)
    {
        if (!File.Exists(arg))
            throw new FileNotFoundException($"Wrong configuration path:\n{arg}");

        configuration.AddJsonFile(arg, optional: true, reloadOnChange: true);
    }

    //AssertConfigurationIsCorrect(configuration);

    return configuration;
}


void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddDbContext<VkActivityContext>(options =>
        options.UseNpgsql(context.Configuration.GetSecretValue("ConnectionStrings:Default")));

    // TODO: remove!
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    // For repositories
    services.AddScoped<IDbContextFactory<VkActivityContext>, VkActivityContextFactory>();

    services.AddScoped<IConnectionAnalyser, ConnectionAnalyser>(sp =>
    {
        var connectionAnalyzer = new ConnectionAnalyser(
            sp.GetService<ILogger<ConnectionAnalyser>>(),
            context.Configuration.GetSection("ConnectionAnalyser:Urls").Get<string[]>());

        if (context.Configuration.GetSection("Proxy:UseProxy")?.Get<bool>() == true)
        {
            connectionAnalyzer.InitializeProxy(context.Configuration["Proxy:Socket"],
                context.Configuration.GetSecretValue("Proxy:Login"),
                context.Configuration.GetSecretValue("Proxy:Password"));

            HttpClient.DefaultProxy = connectionAnalyzer.WebProxy;
        }
        return connectionAnalyzer;
    });

    services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
    services.AddScoped<IUsersRepository, UsersRepository>();

    services.AddScoped<IScheduler, Scheduler>();
    services.AddScoped<IActivityLoggerService, ActivityLoggerService>();

    //services.AddScoped<IUserWatcher, Services.UserWatcher>();

    services.AddHostedService<Worker>();
}
