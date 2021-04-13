using Moq;
using Regi.Abstractions;
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
        private DirectoryInfo _workingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        private readonly Mock<ILogSink> _logSinkMock = new(MockBehavior.Strict);

        protected override ManagedProcess CreateTestClass()
        {
            return new ManagedProcess(_fileName, _arguments, _workingDirectory, _logSinkMock.Object);
        }

        [Fact]
        public async Task Start_success()
        {
            var defaultSB = new StringBuilder();
            var errorSB = new StringBuilder();

            _logSinkMock.Setup(m => m.OutputEventHandler)
                .Returns((o, e) =>
                {
                    defaultSB.Append(e.Data);
                });
            _logSinkMock.Setup(m => m.ErrorEventHandler)
                .Returns((o, e) =>
                {
                    errorSB.Append(e.Data);
                });

            var process = TestClass;

            await process.StartAsync(CancellationToken.None);

            process.Process.WaitForExit(); // only do this for testing purposes

            Assert.Equal(_fileName, process.Process.StartInfo.FileName);
            Assert.Equal(_arguments, process.Process.StartInfo.Arguments);
            Assert.Equal(_workingDirectory.FullName, process.Process.StartInfo.WorkingDirectory);

            Assert.Contains(".NET", defaultSB.ToString());
            Assert.Empty(errorSB.ToString());
        }

        
    }
}
