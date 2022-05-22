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
using VkActivity.Worker;
using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Helpers;
using VkActivity.Worker.Services;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Scheduler;

[assembly: InternalsVisibleTo("UnitTests")]
[assembly: InternalsVisibleTo("IntegrationTests")]


IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureWebHostDefaults(ConfigureWebHostDefaults)
    .ConfigureServices(ConfigureServices)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(host.Services.GetService<IConfiguration>())
    .CreateLogger();

Log.Warning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
    Process.GetCurrentProcess().MainModule?.ModuleName, Environment.MachineName,
    Environment.OSVersion, Environment.UserName, Environment.ProcessId);

await host.RunAsync();

void ConfigureWebHostDefaults(IWebHostBuilder webHostBuilder)
{
    webHostBuilder.ConfigureServices((context, services) =>
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin()
                      .WithMethods("HEAD", "GET", "POST", "PUT", "PATCH", "UPDATE", "DELETE")
                      .AllowAnyHeader()
            );
        });

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
        app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest);

        // Configure the HTTP request pipeline.
        if (!context.HostingEnvironment.IsDevelopment())
            app.UseHsts();

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint(
            context.Configuration[AppSettings.Swagger.EndpointUrl],
            context.Configuration[AppSettings.Swagger.ApiTitle] + " " + context.Configuration[AppSettings.Swagger.ApiVersion])
        );

        app.UseRouting();

        app.UseCors();

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

    services.AddConnectionAnalyzer(context);
    services.AddVkIntegration(context);
    services.AddSingleton<IScheduler, Scheduler>();
    services.AddSingleton<IDelayedLogger, DelayedLogger>();

    services.AddScoped<IUserManager, UserManager>();
    services.AddScoped<IActivityLogger, ActivityLogger>();
    services.AddScoped<IActivityAnalyzer, ActivityAnalyzer>();

    services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
    services.AddScoped<IUsersRepository, UsersRepository>();

    services.AddHostedService<WorkerService>();
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectionAnalyzer(this IServiceCollection services, HostBuilderContext context)
    {
        return services.AddSingleton<IConnectionAnalyser, ConnectionAnalyser>(sp =>
        {
            var connectionAnalyzer = new ConnectionAnalyser(
                sp.GetService<ILogger<ConnectionAnalyser>>(),
                context.Configuration.GetSection(AppSettings.ConnectionAnalyser.Urls).Get<string[]>());

            if (context.Configuration.GetValue<bool>(AppSettings.Proxy.UseProxy) == true)
            {
                connectionAnalyzer.InitializeProxy(context.Configuration[AppSettings.Proxy.Socket],
                    context.Configuration[AppSettings.Proxy.Login],
                    context.Configuration[AppSettings.Proxy.Password]);

                HttpClient.DefaultProxy = connectionAnalyzer.WebProxy;
            }
            return connectionAnalyzer;
        });
    }

    public static IServiceCollection AddVkIntegration(this IServiceCollection services, HostBuilderContext context)
    {
        return services.AddSingleton<IVkIntegration, VkIntegration>(
            sp => new VkIntegration(
                context.Configuration[AppSettings.Vk.AccessToken],
                context.Configuration[AppSettings.Vk.Version])
            );
    }
}

public static class WebHostBuilderExtensions
{

}

