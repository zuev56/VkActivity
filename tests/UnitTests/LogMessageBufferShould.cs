using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VkActivity.Worker.Services;
using Xunit;

namespace UnitTests
{
    public class LogMessageBufferShould
    {
        private const int MessageRepeatTimes = 1000;
        private readonly TimeSpan _logShowInterval = TimeSpan.FromSeconds(1);
        private readonly int _logMessageBufferAnalyzeIntervalMs;
        private readonly Mock<ILogger> _loggerMock = new();

        public LogMessageBufferShould()
        {
            _logMessageBufferAnalyzeIntervalMs = (int)(_logShowInterval.TotalMilliseconds / 2);
        }

        [Theory]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        public async Task Invoke_ILogger_Log_Once(LogLevel logLevel)
        {
            // Arrange
            var testMessage = $"test{logLevel}Message";
            _loggerMock.Setup(x => x.IsEnabled(logLevel)).Returns(true);

            var logMessageBuffer = CreateLogMessageBuffer();
            logMessageBuffer.SetupMessage(testMessage, _logShowInterval);

            // Act
            for (int i = 0; i < MessageRepeatTimes; i++)
                logMessageBuffer.Add(testMessage, logLevel, typeof(LogMessageBufferShould));

            await Task.Delay(_logShowInterval);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }


        private DelayedLogger CreateLogMessageBuffer()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(_loggerMock.Object);

            return new DelayedLogger(loggerFactoryMock.Object, _logMessageBufferAnalyzeIntervalMs);
        }

    }
}
