using Zs.Common.Abstractions;

namespace VkActivity.Worker.Helpers
{
    public static class OperationResultExtensions
    {
        public static void EnsureSuccess(this IOperationResult operationResult)
        {
            // TODO: Create specific Exception and move the code to Zs.Common
            if (!operationResult.IsSuccess)
                throw new Exception(operationResult.JoinMessages());
        }
    }
}
