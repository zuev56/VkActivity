
namespace VkActivity.Worker.Abstractions;

public interface IDelayedLogger
{
    TimeSpan DefaultLogWriteInterval { get; set; }
    void SetupLogMessage(string messageText, TimeSpan logShowInterval);
    int Log<TSourceContext>(string messageText, LogLevel logLevel, TSourceContext sourceContextType) where TSourceContext : Type;
    int LogTrace<TSourceContext>(string messageText, TSourceContext sourceContextType) where TSourceContext : Type;
    int LogDebug<TSourceContext>(string messageText, TSourceContext sourceContextType) where TSourceContext : Type;
    int LogInformation<TSourceContext>(string messageText, TSourceContext sourceContextType) where TSourceContext : Type;
    int LogWarning<TSourceContext>(string message, TSourceContext sourceContextType) where TSourceContext : Type;
    int LogError<TSourceContext>(string message, TSourceContext sourceContextType) where TSourceContext : Type;
    int LogCritical<TSourceContext>(string messageText, TSourceContext sourceContextType) where TSourceContext : Type;
}
