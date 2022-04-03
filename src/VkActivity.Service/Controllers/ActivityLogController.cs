
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkActivity.Service.Abstractions;
using Zs.Common.Extensions;

namespace VkActivity.Service.Controllers;

[Area("vk")]
[Route("api/vk/[controller]")] // глобальный префикс для маршрутов
//[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController] // Реализует проверку модели и возвращает 400, если она не валидна
public class ActivityLogController : Controller
{
    private readonly IActivityLoggerService _activityLoggerService;
    private readonly ILogger<ActivityLogController> _logger;

    internal ActivityLogController(
        IActivityLoggerService activityLoggerService,
        ILogger<ActivityLogController> logger)
    {
        _activityLoggerService = activityLoggerService ?? throw new ArgumentNullException(nameof(activityLoggerService));
        _logger = logger;
    }

    [HttpPost(nameof(AddNewUsers))]
    public async Task<IActionResult> AddNewUsers(int[] userIds)
    {
        return Ok("AddNewUsers called!");
    }

    [HttpGet]
    public IActionResult Test()
    {
        try
        {
            return Ok("Test");
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                InnerExceptionMessage = ex.InnerException?.Message,
                InnerExceptionType = ex.InnerException?.GetType().FullName,
                InnerExceptionStackTrace = ex.InnerException?.StackTrace,
                Message = ex.Message,
                Type = ex.GetType().FullName,
                StackTrace = ex.StackTrace
            });
            //return StatusCode(500);
        }
    }
}
