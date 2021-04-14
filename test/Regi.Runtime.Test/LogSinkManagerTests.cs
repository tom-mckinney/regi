using Moq;
using Regi.Abstractions;
using Regi.Runtime.LogHandlers;
using Regi.Test;
using Regi.Test.Stubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upstream.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Runtime.Test
{
    public class LogSinkManagerTests : TestBase<LogSinkManager>
    {
        private readonly Mock<ILogHandlerFactory> _logHandlerFactory = new(MockBehavior.Strict);
        private readonly TestLogger _testLogger;

        public LogSinkManagerTests(ITestOutputHelper output)
        {
            _testLogger = new TestLogger(output);
        }

        protected override LogSinkManager CreateTestClass()
        {
            return new LogSinkManager(_logHandlerFactory.Object);
        }

        [Fact]
        public async Task Create_instantiates_and_tracks_LogSink()
        {
            _logHandlerFactory.Setup(m => m.CreateLogHandler<SilentLogHandler>())
                .Returns(new SilentLogHandler(_testLogger));
            _logHandlerFactory.Setup(m => m.CreateLogHandler<DefaultLogHandler>())
                .Returns(new DefaultLogHandler(_testLogger));

            var managedProcessId = Guid.NewGuid();
            var sink = await TestClass.CreateAsync(managedProcessId);

            Assert.True(TestClass.LogSinks.ContainsKey(managedProcessId), $"No LogSink with managed process ID {managedProcessId}");
            Assert.Same(sink, TestClass.LogSinks[managedProcessId]);
            Assert.Equal(managedProcessId, sink.ManagedProcessId);
        }

        [Fact]
        public async Task Create_throws_if_ManagedProcess_Id_already_tracked()
        {
            _logHandlerFactory.Setup(m => m.CreateLogHandler<SilentLogHandler>())
                .Returns(new SilentLogHandler(_testLogger));
            _logHandlerFactory.Setup(m => m.CreateLogHandler<DefaultLogHandler>())
                .Returns(new DefaultLogHandler(_testLogger));

            var managedProcessId = Guid.NewGuid();

            TestClass.LogSinks.TryAdd(managedProcessId, new StubbedLogSink()); // pre-existing LogSink

            await Assert.ThrowsAsync<InvalidOperationException>(() => TestClass.CreateAsync(managedProcessId).AsTask());
        }
    }
}
