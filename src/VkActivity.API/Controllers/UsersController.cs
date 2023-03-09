using Microsoft.AspNetCore.Mvc;
using VkActivity.Common.Abstractions;

namespace VkActivity.Api.Controllers;

[Route("api/[controller]")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController]
public sealed class UsersController : Controller
{
    private readonly IUserManager _userManager;


    public UsersController(IUserManager userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    /// <summary>
    /// Get user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUser(int userId)
    {
        var gerUsersResult = await _userManager.GetUserAsync(userId);
        return gerUsersResult.Successful
            ? Ok(gerUsersResult.Value.ToDto())
            : StatusCode(500, gerUsersResult);
    }

    /// <summary>
    /// Add users by theirs Vk-Identifiers
    /// </summary>
    /// <param name="screenNames"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> AddNewUsers([FromBody] string[] screenNames)
    {
        if (screenNames == null || screenNames.Length == 0)
            return BadRequest("No VK user IDs to add");

        var addUsersResult = await _userManager.AddUsersAsync(screenNames).ConfigureAwait(false);

        return addUsersResult.Successful
            ? Ok(addUsersResult)
            : StatusCode(500, addUsersResult);
    }


    [HttpPost("friends/{userId:int}")]
    public async Task<IActionResult> AddFriendsOf(int userId)
    {
        if (userId == default)
        {
            return BadRequest("userId must be present");
        }

        var addFriendsResult = await _userManager.AddFriendsOf(userId);

        return addFriendsResult.Successful
            ? Ok(addFriendsResult)
            : StatusCode(500, addFriendsResult);
    }
}