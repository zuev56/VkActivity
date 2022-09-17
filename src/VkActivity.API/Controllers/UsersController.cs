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


    [HttpPost("friends/{userId:int}")]
    public async Task<IActionResult> AddFriendsOf(int userId)
    {
        if (userId == default)
        {
            return BadRequest("userId must be present");
        }

        var addFriendsResult = await _userManager.AddFriendsOf(userId);

        return addFriendsResult.IsSuccess
            ? Ok(addFriendsResult)
            : StatusCode(500, addFriendsResult);
    }
}
