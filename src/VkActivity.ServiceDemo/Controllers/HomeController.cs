using Microsoft.AspNetCore.Mvc;

namespace VkActivity.Service.Controllers;

[ApiController]
[Route("Home")]
public class HomeController : Controller
{
    ILogger<HomeController> _logger;
    private const string runningMessage = "WindowsServiceApiDemo is running...";

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public string Get()
    {
        _logger.LogInformation(runningMessage);
        return runningMessage;
    }
}
