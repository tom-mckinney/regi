using Regi.Abstractions;
using Regi.Runtime;
using Regi.Runtime.LogHandlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Regi.Test.Stubs
{
    public class StubbedLogSink : LogSink
    {
        public StubbedLogSink(
            ITestOutputHelper testOutput = null,
            Guid managedProcessId = default,
            ILogHandler outputHandler = null,
            ILogHandler errorHandler = null)
            : base(managedProcessId, outputHandler, errorHandler)
        {
            OutputLogHandler ??= new DefaultLogHandler(new TestLogger(testOutput));
            ErrorLogHandler ??= new DefaultLogHandler(new TestLogger(testOutput));
        }
    }
}
