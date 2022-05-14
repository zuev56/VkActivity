using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace VkActivity.Worker.Controllers;

[Route("api/[controller]")]
public sealed class HealthCheckController : Controller
{
    // TODO: Try HealthCheckApplicationBuilderExtensions _healthCheckApplicationBuilderExtensions;

    [HttpGet]
    [HttpHead]
    public IActionResult GetHealthInfo()
    {
        if (Request.Method == "HEAD")
            return Ok();

        var currentProcess = Process.GetCurrentProcess();

        return Ok(new
        {
            ProcessRunningTime = $"{DateTime.Now - currentProcess.StartTime:G}",
            CpuTime = new
            {
                Total = currentProcess.TotalProcessorTime,
                User = currentProcess.UserProcessorTime,
                Priveleged = currentProcess.PrivilegedProcessorTime,
            },
            MemoryUsage = new
            {
                Current = BytesToSize(currentProcess.WorkingSet64),
                Peak = BytesToSize(currentProcess.PeakWorkingSet64)
            },
            ActiveThreads = currentProcess.Threads.Count
        });
    }

    // TODO: Use Zs.Common.Models.ProgramUtilites.GetAppsettingsPath instead
    private static string BytesToSize(long bytes)
    {
        string[] array = new string[5]
        {
            "Bytes",
            "KB",
            "MB",
            "GB",
            "TB"
        };

        if (bytes == 0L)
        {
            return "0 Byte";
        }

        int num = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024.0));
        return Math.Round(bytes / Math.Pow(1024.0, num), 2) + " " + array[num];
    }
}


