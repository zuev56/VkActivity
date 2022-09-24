using Zs.Common.Abstractions;

namespace VkActivity.Worker.Abstractions;

public interface IActivityLogger
{
    Task<IOperationResult> SaveUsersActivityAsync();
    Task<IOperationResult> ChangeAllUserActivitiesToUndefinedAsync();
}
