using Regi.Abstractions;
using Regi.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Regi.Test.Stubs
{
    public class StubbedManagedProcess : ManagedProcess
    {
        public StubbedManagedProcess(
            string fileName = "dotnet",
            string arguments = "--info",
            DirectoryInfo workingDirectory = null,
            ILogSink logSink = null)
            : base(fileName, arguments, workingDirectory, logSink)
        {
            Id = Guid.NewGuid();
            WorkingDirectory ??= new DirectoryInfo(Directory.GetCurrentDirectory());
            LogSink ??= new StubbedLogSink(TestOutput, Id);
        }

        public ITestOutputHelper TestOutput { get; set; }
    }
}
