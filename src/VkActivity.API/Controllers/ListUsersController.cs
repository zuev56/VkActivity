using Microsoft.AspNetCore.Mvc;
using VkActivity.Api.Abstractions;
using Zs.Common.Extensions;

namespace VkActivity.Api.Controllers;

[Route("api/[controller]")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController]
public sealed class ListUsersController : Controller
{
    private readonly IActivityAnalyzer _activityAnalyzer;


    public ListUsersController(
        IActivityAnalyzer activityAnalyzer)
    {
        _activityAnalyzer = activityAnalyzer ?? throw new ArgumentNullException(nameof(activityAnalyzer));
    }

    /// <summary>
    /// Get users list with theirs activity
    /// </summary>
    /// <param name="filterText"></param>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetUsersWithActivity(string? filterText, DateTime fromDate, DateTime toDate)
    {
        var usersWithActivityResult = await _activityAnalyzer.GetUsersWithActivityAsync(filterText, fromDate, toDate);
        usersWithActivityResult.AssertResultIsSuccessful();
        var userDtos = usersWithActivityResult.Value.Select(Mapper.ToListUserDto);

        return Ok(userDtos);
    }
}
