using Zs.Common.Services.Abstractions;

namespace VkActivity.Service;

internal interface IUserWatcher
{
    public IReadOnlyCollection<IJobBase> Jobs { get; }

    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
