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
        IUserManager userManager,
        ILogger<ListUsersController> logger)
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
    public async Task<IActionResult> GetUsersWithActivity(string filterText, DateTime fromDate, DateTime toDate)
    {
        var usersWithActivityResult = await _activityAnalyzer.GetUsersWithActivityAsync(filterText, fromDate, toDate);
        usersWithActivityResult.AssertResultIsSuccessful();
        var userDtos = usersWithActivityResult.Value.Select(Mapper.ToListUserDto);

        return Ok(userDtos);
    }

    /// <summary>
    /// Add users by theirs Vk-Identifiers
    /// </summary>
    /// <param name="vkUserIds"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> AddNewUsers([FromBody] string[] vkUserIds)
    {
        if (vkUserIds == null || vkUserIds.Length == 0)
            return BadRequest("No VK user IDs to add");

        var addUsersResult = await _userManager.AddUsersAsync(vkUserIds).ConfigureAwait(false);

        return addUsersResult.IsSuccess
            ? Ok(addUsersResult)
            : StatusCode(500, addUsersResult);
    }
}
