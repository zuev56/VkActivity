using Microsoft.AspNetCore.Mvc;
using VkActivity.Worker.Abstractions;
using Zs.Common.Extensions;

namespace VkActivity.Worker.Controllers;

[Route("api/[controller]")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController] // Реализует проверку модели и возвращает 400, если она не валидна
public sealed class ActivityLogController : Controller
{
    private readonly IActivityAnalyzer _activityAnalyzerService;

    // + Накапливать ошибки соединения и выдавать их один раз в 10 минут
    // + Анализировать соединение и останавливать запросы к Vk API при разрыве связи
    //+/- Раз в день обновлять пользователей в БД (ТЕСТ)
    //+/- ActivityAnalyzer.GetActivitySeconds учитывать каждую платформу по отдельности (ТЕСТ)
    //+/- Сделать другие контроллеры, необходимые для фронтенда
    //+/- Настроить использование appsettings.Development.json
    //+/- Сделать настройку Kestrel из конфигурационного файла(возможность переопределения заданных в коде параметров)
    // - Покрыть тестами оставшееся!
    // - Проверить корректность работы логгера активности пользователей
    // - Сделать обработку деактивированного пользователя deactivated: string [ deleted, banned ]



    public ActivityLogController(IActivityAnalyzer activityAnalyzerService)
    {
        _activityAnalyzerService = activityAnalyzerService ?? throw new ArgumentNullException(nameof(activityAnalyzerService));
    }

    [HttpGet("{userId:int}/period")]
    public async Task<IActionResult> GetPeriodInfo(
        [FromRoute] int userId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var periodStatisticsResult = await _activityAnalyzerService.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);
        periodStatisticsResult.AssertResultIsSuccessful();
        var periodInfoDto = Mapper.ToPeriodInfoDto(periodStatisticsResult.Value);

        return Ok(periodInfoDto);
    }

    [HttpGet("{userId:int}/fulltime")]
    public async Task<IActionResult> GetFullTimeInfo([FromRoute] int userId)
    {
        var fullTimeStatistictResult = await _activityAnalyzerService.GetFullTimeActivityAsync(userId);
        fullTimeStatistictResult.AssertResultIsSuccessful();
        var fullTimeInfoDto = Mapper.ToFullTimeInfoDto(fullTimeStatistictResult.Value);

        return Ok(fullTimeInfoDto);
    }


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
