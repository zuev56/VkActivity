using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Models.Dto;
using Zs.Common.Extensions;

namespace VkActivity.Service.Controllers;

[Route("api/[controller]")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController] // Реализует проверку модели и возвращает 400, если она не валидна
public sealed class ActivityLogController : Controller
{
    private readonly IActivityAnalyzer _activityAnalyzerService;
    private readonly IMapper _mapper;

    //+/- Сделать другие контроллеры, необходимые для фронтенда
    //+/- Настроить использование appsettings.Development.json
    //- Покрыть тестами оставшееся!
    //- Сделать настройку Kestrel из конфигурационного файла(возможность переопределения заданных в коде параметров)
    //- Проверить корректность работы логгера активности пользователей
    //- Убрать уже неактуальный функционал из UserWatcher и т.д после создания отдельного бота.
    //- Сделать обработку деактивированного пользователя deactivated: string [ deleted, banned ]
    //- Раз в день обновлять пользователей в БД



    public ActivityLogController(
        IActivityAnalyzer activityAnalyzerService,
        IMapper mapper)
    {
        _activityAnalyzerService = activityAnalyzerService ?? throw new ArgumentNullException(nameof(activityAnalyzerService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet("{userId:int}period/{fromDate:DateTime}/{toDate:DateTime}")]
    public async Task<IActionResult> GetPeriodInfo(
        [FromRoute] int userId, [FromRoute] DateTime fromDate, [FromRoute] DateTime toDate)
    {
        var periodStatisticsResult = await _activityAnalyzerService.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);
        periodStatisticsResult.AssertResultIsSuccessful();

        return Ok(_mapper.Map<PeriodInfoDto>(periodStatisticsResult.Value));
    }

    [HttpGet("{userId:int}/fulltime")]
    public async Task<IActionResult> GetFullTimeInfo(int userId)
    {
        var fullTimeStatistictResult = await _activityAnalyzerService.GetFullTimeActivityAsync(userId);
        fullTimeStatistictResult.AssertResultIsSuccessful();

        return Ok(_mapper.Map<FullTimeInfoDto>(fullTimeStatistictResult.Value));
    }



    //[Obsolete("Use UsersController.AddNewUsers")]
    //[HttpPost(nameof(AddNewUsers))]
    //public async Task<IActionResult> AddNewUsers(int[] userIds)
    //{
    //    var addUsersResult = await _activityLoggerService.AddNewUsersAsync(userIds).ConfigureAwait(false);
    //    return addUsersResult.IsSuccess
    //        ? Ok(addUsersResult)
    //        : StatusCode(500, addUsersResult);
    //}


    //[Obsolete("Use UsersController.GetUsersWithActivity")]
    //[HttpGet(nameof(GetUsersWithActivity))]
    //public async Task<IActionResult> GetUsersWithActivity(string filterText, DateTime fromDate, DateTime toDate)
    //{
    //    var usersWithActivityResult = await _activityAnalyzerService.GetUsersWithActivityAsync(filterText, fromDate, toDate);
    //    usersWithActivityResult.AssertResultIsSuccessful();

    //    return Ok(_mapper.Map<List<ListUserDto>>(usersWithActivityResult.Value));
    //}

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

}
