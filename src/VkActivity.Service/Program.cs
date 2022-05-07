using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using VkActivity.Data;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Repositories;
using VkActivity.Service;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Helpers;
using VkActivity.Service.Services;
using Zs.Common.Enums;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;

[assembly: InternalsVisibleTo("UnitTests")]
[assembly: InternalsVisibleTo("IntegrationTests")]

// TODO: recognize environment and pass to CreateConfiguration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(CreateConfiguration(args))
    .CreateLogger();

Log.Warning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
    Process.GetCurrentProcess().MainModule?.ModuleName,
    Environment.MachineName,
    Environment.OSVersion,
    Environment.UserName,
    Environment.ProcessId);


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(ConfigureAppConfiguration)
    .UseSerilog()
    .ConfigureWebHostDefaults(ConfigureWebHostDefaults)
    .ConfigureServices(ConfigureServices)
    .Build();

await host.RunAsync();



void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
{
    var environment = Enum.Parse<AppEnvironment>(context.HostingEnvironment.EnvironmentName);
    var configuration = CreateConfiguration(args, environment);

    builder.AddConfiguration(configuration);
}

IConfiguration CreateConfiguration(string[] args, AppEnvironment environment = default)
{
    var configPath = ProgramUtilites.GetAppsettingsPath(environment);

    if (!File.Exists(configPath))
        throw new FileNotFoundException("Configuration file not found", configPath);

    var configuration = new ConfigurationManager();
    configuration.AddJsonFile(configPath, optional: false, reloadOnChange: true);

    foreach (var arg in args)
    {
        if (!File.Exists(arg))
            throw new FileNotFoundException($"Wrong configuration path:\n{arg}");

        configuration.AddJsonFile(arg, optional: true, reloadOnChange: true);
    }

    //AssertConfigurationIsCorrect(configuration);

    return configuration;
}

void ConfigureWebHostDefaults(IWebHostBuilder webHostBuilder)
{
    webHostBuilder.ConfigureServices((context, services) =>
    {
        services.AddControllers().AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            //o.JsonSerializerOptions.Converters.Add(new JsonBooleanConverter()); Can't setup in tests
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options
           => options.SwaggerDoc(context.Configuration[AppSettings.Swagger.ApiVersion],
            new OpenApiInfo
            {
                Title = context.Configuration[AppSettings.Swagger.ApiTitle],
                Version = context.Configuration[AppSettings.Swagger.ApiVersion]
            })
        );
    })
    .Configure((context, app) =>
    {
        //app.UseHttpLogging();
        app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest);

        // Configure the HTTP request pipeline.
        if (!context.HostingEnvironment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //app.UseHsts();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint(
            context.Configuration[AppSettings.Swagger.EndpointUrl],
            context.Configuration[AppSettings.Swagger.ApiTitle] + " " + context.Configuration[AppSettings.Swagger.ApiVersion])
        );

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "Default",
                pattern: "api/{controller}/{action}/{id?}");

            endpoints.MapControllers();
        });
    })
    .ConfigureKestrel((context, serverOptions) =>
    {
        // https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/servers/kestrel/options?view=aspnetcore-6.0

        serverOptions.Limits.MaxConcurrentConnections = 100;
        serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
        serverOptions.Limits.MaxRequestBodySize = 10 * 1024;
        serverOptions.Limits.MinRequestBodyDataRate =
            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
        serverOptions.Limits.MinResponseDataRate =
            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);

        //serverOptions.Listen(IPAddress.Loopback, 5010);
        //serverOptions.Listen(IPAddress.Loopback, 5001,
        //    listenOptions =>
        //    {
        //        listenOptions.UseHttps("testCert.pfx",
        //            "testPassword");
        //    });
    });
}

void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddDbContext<VkActivityContext>(options =>
        options.UseNpgsql(context.Configuration.GetConnectionString(AppSettings.ConnectionStrings.Default)));

    // For repositories
    services.AddScoped<IDbContextFactory<VkActivityContext>, VkActivityContextFactory>();

    services.AddScoped<ApiExceptionFilter>();

    services.AddSingleton<IScheduler, Scheduler>();

    services.AddSingleton<IVkIntegration, VkIntegration>(
        sp => new VkIntegration(context.Configuration[AppSettings.Vk.AccessToken], context.Configuration[AppSettings.Vk.Version]));

    services.AddScoped<IUserManager, UserManager>();
    services.AddScoped<IActivityLogger, ActivityLogger>();
    services.AddScoped<IActivityAnalyzer, ActivityAnalyzer>();

    services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
    services.AddScoped<IUsersRepository, UsersRepository>();


    services.AddHostedService<WorkerService>();
}
