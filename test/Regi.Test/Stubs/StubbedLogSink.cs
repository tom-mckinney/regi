using Regi.Abstractions;
using Regi.Runtime;
using System;
using Xunit.Abstractions;

namespace Regi.Test.Stubs
{
    public class StubbedLogSink : LogSink
    {
        public StubbedLogSink(
            ITestOutputHelper testOutput = null,
            Guid managedProcessId = default,
            string serviceName = null,
            ILogHandler outputHandler = null,
            ILogHandler errorHandler = null)
            : base(managedProcessId, outputHandler, errorHandler)
        {
            OutputLogHandler ??= new StubbedLogHandler(serviceName, new TestLogger(testOutput));
            ErrorLogHandler ??= new StubbedLogHandler(serviceName, new TestLogger(testOutput));
        }

        public string GetStandardOutput()
        {
            if (OutputLogHandler is ILogHandlerWithCapture stubbedOutputHandler)
            {
                return stubbedOutputHandler.GetCapturedLogMessages();
            }
            else
            {
                return null;
            }
        }

        public string GetStandardError()
        {
            if (ErrorLogHandler is ILogHandlerWithCapture stubbedOutputHandler)
            {
                return stubbedOutputHandler.GetCapturedLogMessages();
            }
            else
            {
                return null;
            }
        }
    }
}
