using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VkActivity.Service.Abstractions;
using VkActivity.Service.Models.Dto;
using Zs.Common.Extensions;

namespace VkActivity.Service.Controllers;

[Route("api/[controller]")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController]
public sealed class ListUsersController : Controller
{
    private readonly IActivityLogger _activityLoggerService;
    private readonly IActivityAnalyzer _activityAnalyzerService;
    private readonly ILogger<ListUsersController> _logger;
    private readonly IMapper _mapper;


    public ListUsersController(
        IActivityLogger activityLoggerService,
        IActivityAnalyzer activityAnalyzerService,
        IMapper mapper,
        ILogger<ListUsersController> logger)
    {
        _activityLoggerService = activityLoggerService ?? throw new ArgumentNullException(nameof(activityLoggerService));
        _activityAnalyzerService = activityAnalyzerService ?? throw new ArgumentNullException(nameof(activityAnalyzerService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
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
        var usersWithActivityResult = await _activityAnalyzerService.GetUsersWithActivityAsync(filterText, fromDate, toDate);
        usersWithActivityResult.AssertResultIsSuccessful();

        return Ok(_mapper.Map<List<ListUserDto>>(usersWithActivityResult.Value));
    }

    /// <summary>
    /// Add users by theirs Vk-Identifiers
    /// </summary>
    /// <param name="vkUserIds"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> AddNewUsers([FromBody] int[] vkUserIds)
    {
        if (vkUserIds == null || vkUserIds.Length == 0)
            return BadRequest("No VK user IDs to add");

        var addUsersResult = await _activityLoggerService.AddNewUsersAsync(vkUserIds).ConfigureAwait(false);

        return addUsersResult.IsSuccess
            ? Ok(addUsersResult)
            : StatusCode(500, addUsersResult);
    }
}
