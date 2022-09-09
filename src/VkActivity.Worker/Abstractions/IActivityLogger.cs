using Zs.Common.Abstractions;

namespace VkActivity.Worker.Abstractions;

public interface IActivityLogger
{
    /// <summary> Activity data collection </summary>
    Task<IOperationResult> SaveUsersActivityAsync();
    Task<IOperationResult> ChangeAllUserActivitiesToUndefinedAsync();
}
