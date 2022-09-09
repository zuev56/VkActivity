using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Services;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Connection;

namespace VkActivity.Worker;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectionAnalyzer(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddSingleton<IConnectionAnalyser, ConnectionAnalyser>(sp =>
        {
            var connectionAnalyzer = new ConnectionAnalyser(
                sp.GetService<ILogger<ConnectionAnalyser>>(),
                configuration.GetSection(AppSettings.ConnectionAnalyser.Urls).Get<string[]>());

            if (configuration.GetValue<bool>(AppSettings.Proxy.UseProxy))
            {
                connectionAnalyzer.InitializeProxy(configuration[AppSettings.Proxy.Socket],
                    configuration[AppSettings.Proxy.Login],
                    configuration[AppSettings.Proxy.Password]);

                HttpClient.DefaultProxy = connectionAnalyzer.WebProxy;
            }
            return connectionAnalyzer;
        });
    }

    public static IServiceCollection AddVkIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddSingleton<IVkIntegration, VkIntegration>(
            sp => new VkIntegration(
                configuration[AppSettings.Vk.AccessToken],
                configuration[AppSettings.Vk.Version])
            );
    }
}