using Microsoft.AspNetCore.Mvc;
using VkActivity.Service.Abstractions;
using Zs.Common.Extensions;

namespace VkActivity.Service.Controllers;

[Route("api/[controller]")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController]
public sealed class ListUsersController : Controller
{
    private readonly IActivityAnalyzer _activityAnalyzer;
    private readonly IUserManager _userManager;


    public ListUsersController(
        IActivityAnalyzer activityAnalyzer,
        IUserManager userManager)
    {
        _activityAnalyzer = activityAnalyzer ?? throw new ArgumentNullException(nameof(activityAnalyzer));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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
