using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zs.Common.Services.Logging.DelayedLogger;

// TODO: Move to Zs.Common.Services
namespace Worker.UnitTests;

public sealed class DelayedLoggerShould
{
    private const int MessageRepeatTimes = 100;
    private readonly TimeSpan _specificLogWriteInterval = TimeSpan.FromSeconds(1);
    private readonly int _logMessageBufferAnalyzeIntervalMs;
    private readonly Mock<ILogger> _loggerMock = new();

    public DelayedLoggerShould()
    {
        _logMessageBufferAnalyzeIntervalMs = (int)(_specificLogWriteInterval.TotalMilliseconds / 2);
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    [InlineData(LogLevel.None)]
    public async Task InvokeILoggerLog_Once_WhenReceiveALotOfTheSameMessages(LogLevel logLevel)
    {
        var testMessage = $"test{logLevel}Message";
        using var delayedLogger = CreateDelayedLogger();
        delayedLogger.SetupLogMessage(testMessage, _specificLogWriteInterval);

        for (var i = 0; i < MessageRepeatTimes; i++)
        {
            switch (logLevel)
            {
                case LogLevel.Trace: delayedLogger.LogTrace(testMessage); break;
                case LogLevel.Debug: delayedLogger.LogDebug(testMessage); break;
                case LogLevel.Information: delayedLogger.LogInformation(testMessage); break;
                case LogLevel.Warning: delayedLogger.LogWarning(testMessage); break;
                case LogLevel.Error: delayedLogger.LogError(testMessage); break;
                case LogLevel.Critical: delayedLogger.LogCritical(testMessage); break;
                case LogLevel.None:
                    logLevel = LogLevel.Warning;
                    delayedLogger.Log(testMessage, logLevel); break;
            }
        }

        await Task.Delay(_specificLogWriteInterval + TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);

        _loggerMock.Verify(logger => logger.Log(
            logLevel,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once());
    }

    [Fact]
    public void ThrowArgumentNullException_When_MessageTextIsNull()
    {
        using var delayedLogger = CreateDelayedLogger();
        var action = () => delayedLogger.Log(null, LogLevel.Information);

        Assert.Throws<ArgumentNullException>(() => action());
    }

    [Fact]
    public async Task UseDefaultLogWriteInterval_When_MessageIsNotSetUp()
    {
        var testMessage = "testMessage";
        var logLevel = LogLevel.Debug;
        using var delayedLogger = CreateDelayedLogger();
        var defaultLogWriteInterval = TimeSpan.FromMilliseconds(3000);
        delayedLogger.DefaultLogWriteInterval = defaultLogWriteInterval;

        for (var i = 0; i < MessageRepeatTimes; i++)
        {
            delayedLogger.Log(testMessage, logLevel);
        }

        var defaultDelayTask = Task.Delay(delayedLogger.DefaultLogWriteInterval + TimeSpan.FromMilliseconds(300));

        await Task.Delay(defaultLogWriteInterval - TimeSpan.FromMilliseconds(300));

        _loggerMock.Verify(logger => logger.Log(
            logLevel,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Never());

        await defaultDelayTask;

        _loggerMock.Verify(logger => logger.Log(
            logLevel,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once());
    }

    [Fact]
    public async Task UseSpecificLogWriteInterval_When_MessageIsSetUp()
    {
        var testMessage = $"testMessage";
        var logLevel = LogLevel.Debug;
        using var delayedLogger = CreateDelayedLogger();
        delayedLogger.DefaultLogWriteInterval = TimeSpan.FromMilliseconds(3000);
        delayedLogger.SetupLogMessage(testMessage, _specificLogWriteInterval);

        for (var i = 0; i < MessageRepeatTimes; i++)
        {
            delayedLogger.Log(testMessage, logLevel);
        }

        var specificDelayTask = Task.Delay(delayedLogger.DefaultLogWriteInterval);

        await Task.Delay(_specificLogWriteInterval - TimeSpan.FromMilliseconds(300));

        _loggerMock.Verify(logger => logger.Log(
            logLevel,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Never());

        await specificDelayTask;

        _loggerMock.Verify(logger => logger.Log(
            logLevel,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once());
    }

    private DelayedLogger<DelayedLoggerShould> CreateDelayedLogger()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(_loggerMock.Object);

        return new DelayedLogger<DelayedLoggerShould>(loggerFactoryMock.Object, _logMessageBufferAnalyzeIntervalMs);
    }
}