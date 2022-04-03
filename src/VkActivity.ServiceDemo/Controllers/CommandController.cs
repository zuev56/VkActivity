using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace VkActivity.Service.Controllers;

[ApiController]
[Route("Command/{apiCommand}")]
public class CommandController : Controller
{
    ILogger<CommandController> _logger;
    private string RunningMessage() => $"apiConnamd: {Worker.ApiCommand}";

    public CommandController(ILogger<CommandController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public string SetCommand(string apiCommand)
    {
        Worker.ApiCommand = apiCommand;
        _logger.LogInformation(RunningMessage());
        return RunningMessage();
    }
}
