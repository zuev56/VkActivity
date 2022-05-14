using Microsoft.AspNetCore.Mvc;
using VkActivity.Service.Abstractions;

namespace VkActivity.Service.Controllers;

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
}
