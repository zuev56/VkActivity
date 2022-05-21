using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VkActivity.Worker.Services;
using Xunit;

namespace UnitTests
{
    public class DelayedLoggerShould
    {
        private const int MessageRepeatTimes = 100;
        private readonly TimeSpan _specificLogWriteInterval = TimeSpan.FromSeconds(1);
        private readonly int _logMessageBufferAnalyzeIntervalMs;
        private readonly Type _sourceContextType = typeof(DelayedLoggerShould);
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
            // Arrange
            var testMessage = $"test{logLevel}Message";
            using var delayedLogger = CreateDelayedLogger();
            delayedLogger.SetupLogMessage(testMessage, _specificLogWriteInterval);

            // Act
            for (int i = 0; i < MessageRepeatTimes; i++)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace: delayedLogger.LogTrace(testMessage, _sourceContextType); break;
                    case LogLevel.Debug: delayedLogger.LogDebug(testMessage, _sourceContextType); break;
                    case LogLevel.Information: delayedLogger.LogInformation(testMessage, _sourceContextType); break;
                    case LogLevel.Warning: delayedLogger.LogWarning(testMessage, _sourceContextType); break;
                    case LogLevel.Error: delayedLogger.LogError(testMessage, _sourceContextType); break;
                    case LogLevel.Critical: delayedLogger.LogCritical(testMessage, _sourceContextType); break;
                    case LogLevel.None:
                        logLevel = LogLevel.Warning;
                        delayedLogger.Log(testMessage, logLevel, _sourceContextType); break;
                }
            }

            await Task.Delay(_specificLogWriteInterval + _specificLogWriteInterval / 5);

            // Assert
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
            // Arrange
            using var delayedLogger = CreateDelayedLogger();
            var action = () => delayedLogger.Log(null, LogLevel.Information, typeof(DelayedLoggerShould));

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public async Task UseDefaultLogWriteInterval_When_MessageIsNotSetUp()
        {
            // Arrange
            var testMessage = $"testMessage";
            var logLevel = LogLevel.Debug;
            using var delayedLogger = CreateDelayedLogger();
            var defaultLogWriteInterval = TimeSpan.FromMilliseconds(3000);
            delayedLogger.DefaultLogWriteInterval = defaultLogWriteInterval;

            // Act
            for (int i = 0; i < MessageRepeatTimes; i++)
                delayedLogger.Log(testMessage, logLevel, _sourceContextType);

            var defaultDelayTask = Task.Delay(delayedLogger.DefaultLogWriteInterval);

            await Task.Delay(defaultLogWriteInterval - TimeSpan.FromMilliseconds(300));

            // Assert
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
            // Arrange
            var testMessage = $"testMessage";
            var logLevel = LogLevel.Debug;
            using var delayedLogger = CreateDelayedLogger();
            delayedLogger.DefaultLogWriteInterval = TimeSpan.FromMilliseconds(3000);
            delayedLogger.SetupLogMessage(testMessage, _specificLogWriteInterval);

            // Act
            for (int i = 0; i < MessageRepeatTimes; i++)
                delayedLogger.Log(testMessage, logLevel, _sourceContextType);

            var specificDelayTask = Task.Delay(delayedLogger.DefaultLogWriteInterval);

            await Task.Delay(_specificLogWriteInterval - TimeSpan.FromMilliseconds(300));

            // Assert
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



        private DelayedLogger CreateDelayedLogger()
        {
            _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(_loggerMock.Object);

            return new DelayedLogger(loggerFactoryMock.Object, _logMessageBufferAnalyzeIntervalMs);
        }

    }
}
