using Microsoft.Extensions.Logging;
using Moq;
using Regi.Abstractions;
using Regi.Test;
using Upstream.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Runtime.Test
{
    public class LogHandlerFactoryTests : TestBase<LogHandlerFactory>
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock = new(MockBehavior.Strict);
        private readonly TestLogger<TestLogHandler> _testLogger;

        public LogHandlerFactoryTests(ITestOutputHelper output)
        {
            _testLogger = new(output);
        }

        protected override LogHandlerFactory CreateTestClass()
        {
            return new LogHandlerFactory(_loggerFactoryMock.Object);
        }

        [Fact]
        public void Create_success()
        {
            _loggerFactoryMock.Setup(m => m.CreateLogger(It.Is<string>(n => n.Contains(nameof(TestLogHandler)))))
                .Returns(_testLogger);

            var logHandler = TestClass.CreateLogHandler<TestLogHandler>();

            logHandler.Handle(LogLevel.Information, "Foo");

            Assert.Contains(_testLogger.Entries, e => e.Message == "Foo");
        }

        private class TestLogHandler : LogHandlerBase
        {
            public TestLogHandler(ILogger logger)
                : base(logger)
            {
            }

            public override void Handle(LogLevel logLevel, string message, params object[] args)
            {
                Logger.Log(logLevel, message, args);
            }
        }
    }
}
