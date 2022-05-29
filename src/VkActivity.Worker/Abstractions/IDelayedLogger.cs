
namespace VkActivity.Worker.Abstractions;

public interface IDelayedLogger<TSourceContext>
{
    TimeSpan DefaultLogWriteInterval { get; set; }
    void SetupLogMessage(string messageText, TimeSpan logShowInterval);
    int Log(string messageText, LogLevel logLevel);
    int LogTrace(string messageText);
    int LogDebug(string messageText);
    int LogInformation(string messageText);
    int LogWarning(string message);
    int LogError(string message);
    //int LogError<TSourceContext>(Exception exception, string message);
    int LogCritical(string messageText);
}
