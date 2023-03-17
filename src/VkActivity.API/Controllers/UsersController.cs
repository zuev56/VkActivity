using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VkActivity.Api.Models;
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
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser(int userId)
    {
        var gerUsersResult = await _userManager.GetUserAsync(userId);
        var userDto = gerUsersResult.Value.ToDto();

        return Ok(userDto);
    }

    /// <summary>
    /// Add users by theirs Vk-Identifiers
    /// </summary>
    /// <param name="screenNames"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddNewUsers([FromBody] string[] screenNames)
    {
        if (screenNames.Length == 0)
        {
            return BadRequest("No VK user IDs to add");
        }

        var addUsersResult = await _userManager.AddUsersAsync(screenNames).ConfigureAwait(false);
        var userDtos = addUsersResult.Value.Select(Mapper.ToDto).ToArray();

        return Ok(userDtos);
    }


    [HttpPost("friends/{userId:int}")]
    [ProducesResponseType(typeof(UserDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddFriendsOf(int userId)
    {
        if (userId == default)
        {
            return BadRequest("userId must be present");
        }

        var addFriendsResult = await _userManager.AddFriendsOf(userId);
        var userDtos = addFriendsResult.Value.Select(Mapper.ToDto).ToArray();

        return Ok(userDtos);
    }
}