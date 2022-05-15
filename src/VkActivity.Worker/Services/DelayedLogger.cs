using System.Collections.Concurrent;
using System.Collections.Immutable;
using VkActivity.Worker.Abstractions;
using Zs.Common.Extensions;

namespace VkActivity.Worker.Services;

public sealed class DelayedLogger : IDelayedLogger, IDisposable
{
    public TimeSpan DefaultInterval { get; set; } = TimeSpan.FromMinutes(1);

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
                case LogLevel.Warning: logger.LogWarningIfNeed(summaryMessage); break;
                case LogLevel.Error: logger.LogErrorIfNeed(summaryMessage); break;
            }

            _messages = _messages.RemoveAll(m => m.Text == messageInfo.Message.Text);
        }
    }

    public void SetupMessage(string message, TimeSpan logShowInterval)
    {
        _messageTemplatesWithInterval.AddOrUpdate(message, logShowInterval, (k, v) => v);
    }

    public int AddWarning<TSourceContext>(string message, TSourceContext sourceContextType)
        where TSourceContext : Type
    {
        return Add(message, LogLevel.Warning, sourceContextType);
    }

    public int AddError<TSourceContext>(string message, TSourceContext sourceContextType)
        where TSourceContext : Type
    {
        ArgumentNullException.ThrowIfNull(message);

        return Add(message, LogLevel.Error, sourceContextType);
    }

    public int Add<TSourceContext>(string messageText, LogLevel logLevel, TSourceContext sourceContextType)
        where TSourceContext : Type
    {
        if (!_messageTemplatesWithInterval.ContainsKey(messageText))
            SetupMessage(messageText, DefaultInterval);

        _messages = _messages.Add(new(messageText, logLevel, DateTime.UtcNow, sourceContextType));

        return _messages.Count(m => m.Text == messageText);
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
