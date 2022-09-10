using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Zs.Common.Extensions;

namespace VkActivity.Api;

// TODO: Move to Zs.Common.Web
public class ApiExceptionFilter : Attribute, IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogErrorIfNeed(context.Exception, $"Action {context.ActionDescriptor.DisplayName} error");

        context.Result = new ContentResult
        {
            StatusCode = 500,
            Content = "Internal Server Error"
        };

        context.ExceptionHandled = true;
    }
}
