using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VkActivity.Common;
using VkActivity.Data.Services;

namespace VkActivity.Api.Controllers;

[Route("api/[controller]")]
public sealed class HealthCheckController : Controller
{
    // TODO: Try HealthCheckApplicationBuilderExtensions _healthCheckApplicationBuilderExtensions;
    private readonly string _connectionString;

    public HealthCheckController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString(AppSettings.ConnectionStrings.Default)!;
    }

    [HttpGet]
    [HttpHead]
    public async Task<IActionResult> GetHealthInfo()
    {
        if (Request.Method == "HEAD")
        {
            return Ok();
        }

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
            ActiveThreads = currentProcess.Threads.Count,
            Database = await DbInfoService.GetInfoAsync(_connectionString)
        });
    }

    // TODO: Use Zs.Common.Models.ProgramUtilites.GetAppsettingsPath instead
    private static string BytesToSize(long bytes)
    {
        var array = new string[5]
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

        var num = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024.0));
        return Math.Round(bytes / Math.Pow(1024.0, num), 2) + " " + array[num];
    }
}