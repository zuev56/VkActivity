using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VkActivity.Common.Abstractions;
using VkActivity.Common.Services;

namespace VkActivity.Common;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVkIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddSingleton<IVkIntegration, VkIntegration>(
            sp => new VkIntegration(
                configuration[AppSettings.Vk.AccessToken],
                configuration[AppSettings.Vk.Version])
            );
    }
}