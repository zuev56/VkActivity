
namespace VkActivity.Worker.Abstractions;

public interface IDelayedLogger
{
    TimeSpan DefaultInterval { get; set; }

    int Add<TSourceContext>(string messageText, LogLevel logLevel, TSourceContext sourceContextType) where TSourceContext : Type;
    int AddError<TSourceContext>(string message, TSourceContext sourceContextType) where TSourceContext : Type;
    int AddWarning<TSourceContext>(string message, TSourceContext sourceContextType) where TSourceContext : Type;
    void SetupMessage(string message, TimeSpan logShowInterval);
}
