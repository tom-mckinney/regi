using Moq;
using Regi.Abstractions;
using Regi.Test.Stubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Upstream.Testing;
using Xunit;

namespace Regi.Runtime.Test
{
    public class ManagedProcessTests : TestBase<ManagedProcess>
    {
        private string _fileName = "dotnet";
        private string _arguments = "--info";
        private DirectoryInfo _workingDirectory = new(Directory.GetCurrentDirectory());
        private readonly StubbedLogSink _logSinkStub = new();
        private readonly Mock<IRuntimeInfo> _runtimeInfoMock = new(MockBehavior.Strict);

        protected override ManagedProcess CreateTestClass()
        {
            return new ManagedProcess(_fileName, _arguments, _workingDirectory, _logSinkStub, _runtimeInfoMock.Object);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Start_success(bool isWindows)
        {
            _runtimeInfoMock.Setup(m => m.IsWindows)
                .Returns(isWindows);

            var process = TestClass;

            var result = await process.StartAsync(CancellationToken.None);

            Assert.Equal(0, result.ExitCode);
            Assert.Contains(".NET", result.StandardOutput);
            Assert.Empty(result.StandardError);

            Assert.Equal(_fileName, process.Process.StartInfo.FileName);
            Assert.Equal(_arguments, process.Process.StartInfo.Arguments);
            Assert.Equal(_workingDirectory.FullName, process.Process.StartInfo.WorkingDirectory);

            Assert.Equal(!isWindows, process.Process.StartInfo.CreateNoWindow);
        }
    }
}
