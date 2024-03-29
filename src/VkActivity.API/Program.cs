using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using VkActivity.Api;
using VkActivity.Api.Abstractions;
using VkActivity.Api.Services;
using VkActivity.Common;
using VkActivity.Common.Abstractions;
using VkActivity.Common.Services;
using VkActivity.Data;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Repositories;

[assembly: InternalsVisibleTo("Api.UnitTests")]
[assembly: InternalsVisibleTo("Api.IntegrationTests")]


var host = Host.CreateDefaultBuilder(args)
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


static void ConfigureWebHostDefaults(IWebHostBuilder webHostBuilder)
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

        services.AddControllers()
            .AddJsonOptions(o =>
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

        services.Configure<RouteOptions>(o => o.LowercaseUrls = true);
    })
    .Configure((context, app) =>
    {
        app.UseSerilogRequestLogging(
            opts => opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest);

        // Configure the HTTP request pipeline.
        if (!context.HostingEnvironment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint(
            context.Configuration[AppSettings.Swagger.EndpointUrl],
            context.Configuration[AppSettings.Swagger.ApiTitle] + " " + context.Configuration[AppSettings.Swagger.ApiVersion])
        );

        app.UseRouting();

        app.UseCors();
        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "Default",
                pattern: "api/{controller}/{action}/{id?}");

            endpoints.MapControllers();
        });
    })
    .ConfigureKestrel((_, serverOptions) =>
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
    var configuration = context.Configuration;

    services.AddDbContext<VkActivityContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString(AppSettings.ConnectionStrings.Default)));

    services.AddScoped<IDbContextFactory<VkActivityContext>, VkActivityContextFactory>();

    services.AddScoped<ApiExceptionFilter>();

    services.AddVkIntegration(configuration);

    services.AddScoped<IUserManager, UserManager>();
    services.AddScoped<IActivityAnalyzer, ActivityAnalyzer>();

    services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
    services.AddScoped<IUsersRepository, UsersRepository>();
}