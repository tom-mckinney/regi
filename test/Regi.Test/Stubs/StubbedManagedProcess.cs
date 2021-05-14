using Regi.Abstractions;
using Regi.Runtime;
using System;
using System.IO;
using Xunit.Abstractions;

namespace Regi.Test.Stubs
{
    public class StubbedManagedProcess : ManagedProcess
    {
        public StubbedManagedProcess(
            string fileName = "dotnet",
            string arguments = "--info",
            DirectoryInfo workingDirectory = null,
            ILogSink logSink = null,
            IRuntimeInfo runtimeInfo = null)
            : base(fileName, arguments, workingDirectory, logSink, runtimeInfo)
        {
            Id = Guid.NewGuid();
            WorkingDirectory ??= new DirectoryInfo(Directory.GetCurrentDirectory());
            LogSink ??= new StubbedLogSink(TestOutput, Id);
            RuntimeInfo ??= new StubbedRuntimeInfo();
        }

        public ITestOutputHelper TestOutput { get; set; }
    }
}
