using System.Diagnostics;
using System.Net;
using AutoMapper;
using Home.Data.Repositories;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using VkActivity.Data;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Services;
using Zs.Common.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(CreateConfiguration(args), "Serilog")
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
    var configuration = CreateConfiguration(args);

    builder.AddConfiguration(configuration);
}

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

void ConfigureWebHostDefaults(IWebHostBuilder webHostBuilder)
{
    webHostBuilder.ConfigureServices((context, services) =>
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options
           => options.SwaggerDoc(context.Configuration["Swagger:ApiVersion"],
            new OpenApiInfo
            {
                Title = context.Configuration["Swagger:ApiTitle"],
                Version = context.Configuration["Swagger:ApiVersion"]
            })
        );
    })
    .Configure((context, app) =>
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint(
            context.Configuration["Swagger:EndpointUrl"],
            context.Configuration["Swagger:ApiTitle"] + " " + context.Configuration["Swagger:ApiVersion"])
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
    .ConfigureKestrel(serverOptions =>
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

        serverOptions.Listen(IPAddress.Loopback, 5000);
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
        options.UseNpgsql(context.Configuration.GetSecretValue("ConnectionStrings:Default")));

    // TODO: remove!
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    // For repositories
    services.AddScoped<IDbContextFactory<VkActivityContext>, VkActivityContextFactory>();

    var mapperConfig = MapperConfiguration.CreateMapperConfiguration();
    mapperConfig.AssertConfigurationIsValid();
    services.AddScoped<IMapper, Mapper>(sp => new Mapper(mapperConfig));

    services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
    services.AddScoped<IUsersRepository, UsersRepository>();

    services.AddScoped<IActivityLoggerService, ActivityLoggerService>();

    services.AddSingleton<IScheduler, Scheduler>();

    services.AddHostedService<UserWatcher>();
}
