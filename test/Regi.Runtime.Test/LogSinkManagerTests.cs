using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upstream.Testing;
using Xunit;

namespace Regi.Runtime.Test
{
    public class LogSinkManagerTests : TestBase<LogSinkManager>
    {
        protected override LogSinkManager CreateTestClass()
        {
            return new LogSinkManager();
        }

        [Fact]
        public async Task Create_instantiates_and_tracks_LogSink()
        {
            var managedProcessId = Guid.NewGuid();
            var sink = await TestClass.CreateAsync(managedProcessId);

            Assert.True(TestClass.LogSinks.ContainsKey(managedProcessId), $"No LogSink with managed process ID {managedProcessId}");
            Assert.Same(sink, TestClass.LogSinks[managedProcessId]);
            Assert.Equal(managedProcessId, sink.ManagedProcessId);
        }

        [Fact]
        public async Task Create_throws_if_ManagedProcess_Id_already_tracked()
        {
            var managedProcessId = Guid.NewGuid();

            TestClass.LogSinks.TryAdd(managedProcessId, new LogSink()); // pre-existing LogSink

            await Assert.ThrowsAsync<InvalidOperationException>(() => TestClass.CreateAsync(managedProcessId).AsTask());
        }
    }
}
