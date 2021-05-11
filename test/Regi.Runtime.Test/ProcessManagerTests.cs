using Moq;
using Regi.Abstractions;
using Regi.Test.Stubs;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Upstream.Testing;
using Xunit;

namespace Regi.Runtime.Test
{
    public class ProcessManagerTests : TestBase<ProcessManager>
    {
        private readonly Mock<ILogSinkManager> _logSinkManagerMock = new(MockBehavior.Strict);
        private readonly Mock<IFileSystem> _fileSystemMock = new(MockBehavior.Strict);

        protected override ProcessManager CreateTestClass()
        {
            return new ProcessManager(_logSinkManagerMock.Object, _fileSystemMock.Object);
        }

        [Fact]
        public async Task Create_returns_ManagedProcess_success()
        {
            var serviceName = "foo";
            var fileName = "foo.exe";
            var arguments = "bar wumbo";
            var workingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            var logSink = new StubbedLogSink();

            _logSinkManagerMock.Setup(m => m.CreateAsync(serviceName, It.IsAny<Guid>()))
                .ReturnsAsync(logSink);

            var managedProcess = await TestClass.CreateAsync(serviceName, fileName, arguments, workingDirectory);

            Assert.NotEqual(default, managedProcess.Id);
            Assert.Equal(fileName, managedProcess.FileName);
            Assert.Equal(arguments, managedProcess.Arguments);
            Assert.Equal(workingDirectory, managedProcess.WorkingDirectory);
            Assert.Same(logSink, ((ManagedProcess)managedProcess).LogSink);
        }

        [Fact]
        public async Task Create_uses_FileSystem_if_no_working_directory_is_specified()
        {
            var serviceName = "foo";
            var fileName = "foo.exe";
            var arguments = "bar wumbo";

            var workingDirectory = Directory.GetCurrentDirectory();

            _logSinkManagerMock.Setup(m => m.CreateAsync(serviceName, It.IsAny<Guid>()))
                .ReturnsAsync(new StubbedLogSink());

            _fileSystemMock.Setup(m => m.WorkingDirectory)
                .Returns(workingDirectory);

            var managedProcess = await TestClass.CreateAsync(serviceName, fileName, arguments, null);

            Assert.Equal(fileName, managedProcess.FileName);
            Assert.Equal(arguments, managedProcess.Arguments);
            Assert.Equal(workingDirectory, managedProcess.WorkingDirectory.FullName);
        }

        [Fact]
        public async Task Create_stores_process_in_ManagedProcesses()
        {
            var serviceName = "foo";
            var fileName = "foo.exe";
            var arguments = "bar wumbo";
            var workingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            _logSinkManagerMock.Setup(m => m.CreateAsync(serviceName, It.IsAny<Guid>()))
                .ReturnsAsync(new StubbedLogSink());

            var managedProcess = await TestClass.CreateAsync(serviceName, fileName, arguments, workingDirectory);

            Assert.True(TestClass.ManagedProcesses.ContainsKey(managedProcess.Id), "Does not have process ID in ManagedProcesses dictionary");
            Assert.Same(managedProcess, TestClass.ManagedProcesses[managedProcess.Id]);
        }

        [Fact]
        public async Task Shutdown_kills_ManagedProcess()
        {
            var processId = Guid.NewGuid();
            var managedProcessMock = new Mock<IManagedProcess>(MockBehavior.Strict);

            managedProcessMock.Setup(m => m.KillAsync(CancellationToken.None))
                .Returns(Task.CompletedTask);

            ((ConcurrentDictionary<Guid, IManagedProcess>)TestClass.ManagedProcesses).TryAdd(processId, managedProcessMock.Object);

            var output = await TestClass.ShutdownAsync(processId, CancellationToken.None);

            Assert.True(output);
            Assert.Empty(TestClass.ManagedProcesses);

            managedProcessMock.VerifyAll();
        }

        [Fact]
        public async Task Shutdown_throws_if_Id_not_managed()
        {
            var shutdownTask = TestClass.ShutdownAsync(Guid.NewGuid(), CancellationToken.None);

            await Assert.ThrowsAsync<InvalidOperationException>(() => shutdownTask.AsTask());
        }
    }
}
