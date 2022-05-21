using System.Collections.Concurrent;
using System.Collections.Immutable;
using VkActivity.Worker.Abstractions;
using Zs.Common.Extensions;

namespace VkActivity.Worker.Services;

public sealed class DelayedLogger : IDelayedLogger, IDisposable
{
    public TimeSpan DefaultLogWriteInterval { get; set; } = TimeSpan.FromMinutes(1);

    private record class Message(
        string Text,
        LogLevel LogLevel,
        DateTime CreateAt,
        Type SourceContextType
    );

    private readonly ConcurrentDictionary<string, TimeSpan> _messageTemplatesWithInterval = new();
    private ImmutableList<Message> _messages = ImmutableList.Create<Message>();
    private readonly Timer _timer;

    private readonly ILoggerFactory _loggerFactory;

    public DelayedLogger(ILoggerFactory loggerFactory, int analyzeIntervalMs = 5)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        _timer = new(new TimerCallback(DoWork), null, 0, analyzeIntervalMs);
    }

    private void DoWork(object? state)
    {
        var expiredMessageInfos = _messages
            .GroupBy(m => m.Text)
            .Select(group => new
            {
                Message = group.OrderBy(m => m.CreateAt).First(),
                Count = group.Count()
            })
            .Where(s => DateTime.UtcNow.Subtract(s.Message.CreateAt) >= _messageTemplatesWithInterval[s.Message.Text]);

        foreach (var messageInfo in expiredMessageInfos)
        {
            var summaryMessage = $"{messageInfo.Message.LogLevel} '{messageInfo.Message.Text}' "
                + $"occured {messageInfo.Count} times since {messageInfo.Message.CreateAt.ToLocalTime()}";

            var logger = _loggerFactory.CreateLogger(messageInfo.Message.SourceContextType);

            switch (messageInfo.Message.LogLevel)
            {
                case LogLevel.Trace: logger.LogTraceIfNeed(summaryMessage); break;
                case LogLevel.Debug: logger.LogDebugIfNeed(summaryMessage); break;
                case LogLevel.Information: logger.LogInformationIfNeed(summaryMessage); break;
                case LogLevel.Warning: logger.LogWarningIfNeed(summaryMessage); break;
                case LogLevel.Error: logger.LogErrorIfNeed(summaryMessage); break;
                case LogLevel.Critical: logger.LogCriticalIfNeed(summaryMessage); break;
            }

            _messages = _messages.RemoveAll(m => m.Text == messageInfo.Message.Text);
        }
    }

    public void SetupLogMessage(string messageText, TimeSpan logShowInterval)
    {
        _messageTemplatesWithInterval.AddOrUpdate(
            messageText, logShowInterval, (key, value) => value);
    }

    public int Log<TSourceContext>(string messageText, LogLevel logLevel, TSourceContext sourceContextType)
        where TSourceContext : Type
    {
        ArgumentNullException.ThrowIfNull(messageText);

        if (!_messageTemplatesWithInterval.ContainsKey(messageText))
            SetupLogMessage(messageText, DefaultLogWriteInterval);

        _messages = _messages.Add(new(messageText, logLevel, DateTime.UtcNow, sourceContextType));

        return _messages.Count(m => m.Text == messageText);
    }

    public int LogTrace<TSourceContext>(string messageText, TSourceContext sourceContextType)
        where TSourceContext : Type
        => Log(messageText, LogLevel.Trace, sourceContextType);

    public int LogInformation<TSourceContext>(string messageText, TSourceContext sourceContextType)
        where TSourceContext : Type
        => Log(messageText, LogLevel.Information, sourceContextType);

    public int LogDebug<TSourceContext>(string messageText, TSourceContext sourceContextType)
        where TSourceContext : Type
        => Log(messageText, LogLevel.Debug, sourceContextType);

    public int LogWarning<TSourceContext>(string messageText, TSourceContext sourceContextType)
        where TSourceContext : Type
        => Log(messageText, LogLevel.Warning, sourceContextType);

    public int LogError<TSourceContext>(string messageText, TSourceContext sourceContextType)
        where TSourceContext : Type
        => Log(messageText, LogLevel.Error, sourceContextType);

    public int LogCritical<TSourceContext>(string messageText, TSourceContext sourceContextType)
        where TSourceContext : Type
        => Log(messageText, LogLevel.Critical, sourceContextType);

    public void Dispose()
    {
        _timer.Dispose();
    }
}
