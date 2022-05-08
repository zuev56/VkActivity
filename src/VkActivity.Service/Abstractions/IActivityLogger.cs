using Zs.Common.Abstractions;

namespace VkActivity.Service.Abstractions;

public interface IActivityLogger
{
    /// <summary> Activity data collection </summary>
    Task<IOperationResult> SaveVkUsersActivityAsync();
}
