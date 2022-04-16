using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Models;
using Zs.Common.Extensions;

namespace VkActivity.Service.Controllers;

[Route("api/[controller]")] // глобальный префикс для маршрутов
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController] // Реализует проверку модели и возвращает 400, если она не валидна
public class ActivityLogController : Controller
{
    private readonly IActivityLoggerService _activityLoggerService;
    private readonly IActivityAnalyzerService _activityAnalyzerService;
    private readonly ILogger<ActivityLogController> _logger;
    private readonly IMapper _mapper;

    public ActivityLogController(
        IActivityLoggerService activityLoggerService,
        IActivityAnalyzerService activityAnalyzerService,
        IMapper mapper,
        ILogger<ActivityLogController> logger)
    {
        _activityLoggerService = activityLoggerService ?? throw new ArgumentNullException(nameof(activityLoggerService));
        _activityAnalyzerService = activityAnalyzerService ?? throw new ArgumentNullException(nameof(activityAnalyzerService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    [HttpPost(nameof(AddNewUsers))]
    public async Task<IActionResult> AddNewUsers(int[] userIds)
    {
        var addUsersResult = await _activityLoggerService.AddNewUsersAsync(userIds).ConfigureAwait(false);
        return addUsersResult.IsSuccess
            ? Ok(addUsersResult)
            : StatusCode(500, addUsersResult);
    }


    [HttpGet(nameof(GetUsersWithActivity))]
    public async Task<IActionResult> GetUsersWithActivity(string filterText, DateTime fromDate, DateTime toDate)
    {
        var usersWithActivityResult = await _activityAnalyzerService.GetUsersWithActivityAsync(filterText, fromDate, toDate);
        usersWithActivityResult.AssertResultIsSuccessful();

        return Ok(_mapper.Map<List<ListUserDto>>(usersWithActivityResult.Value));
    }

    //[HttpGet(nameof(GetUsersWithActivity))]
    //public async Task<IActionResult> GetUsersWithActivity(TableParameters requestParameters)
    //{
    //    // TODO: In future...
    //    throw new NotImplementedException();
    //    //var usersWithActivityResult = await _vkActivityService.GetUsersWithActivityTable(requestParameters);
    //    //usersWithActivityResult.AssertResultIsSuccessful();
    //    //
    //    //return Ok(_mapper.Map<List<ListUserDto>>(usersWithActivityResult.Value));
    //}


    [HttpGet(nameof(GetPeriodInfo))]
    public async Task<IActionResult> GetPeriodInfo(int userId, DateTime fromDate, DateTime toDate)
    {
        var periodStatisticsResult = await _activityAnalyzerService.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);
        periodStatisticsResult.AssertResultIsSuccessful();

        return Ok(_mapper.Map<PeriodInfoDto>(periodStatisticsResult.Value));
    }

    [HttpGet(nameof(GetFullTimeInfo))]
    public async Task<IActionResult> GetFullTimeInfo(int userId)
    {
        var fullTimeStatistictResult = await _activityAnalyzerService.GetFullTimeUserStatisticsAsync(userId);
        fullTimeStatistictResult.AssertResultIsSuccessful();

        return Ok(_mapper.Map<FullTimeInfoDto>(fullTimeStatistictResult.Value));
    }




    [HttpGet(nameof(Test))]
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
