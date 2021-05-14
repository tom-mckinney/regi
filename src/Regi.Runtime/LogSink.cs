using Microsoft.Extensions.Logging;
using Regi.Abstractions;
using System;
using System.Diagnostics;

namespace Regi.Runtime
{
    public class LogSink : ILogSink
    {
        public LogSink(Guid managedProcessId, ILogHandler outputLogHandler, ILogHandler errorLogHandler)
        {
            ManagedProcessId = managedProcessId;
            OutputLogHandler = outputLogHandler;
            ErrorLogHandler = errorLogHandler;
        }

        public Guid ManagedProcessId { get; private set; }

        public ILogHandler OutputLogHandler { get; set; }
        public ILogHandler ErrorLogHandler { get; set; }

        public DataReceivedEventHandler OutputEventHandler => (o, e) => OutputLogHandler.Handle(LogLevel.Information, e.Data);

        public DataReceivedEventHandler ErrorEventHandler => (o, e) => OutputLogHandler.Handle(LogLevel.Error, e.Data);

        public bool TryGetStandardOutput(out string standardOutput)
        {
            if (OutputLogHandler is ILogHandlerWithCapture outputLogHandlerWithCapture)
            {
                standardOutput = outputLogHandlerWithCapture.GetCapturedLogMessages();
                return true;
            }

            standardOutput = null;
            return false;
        }

        public bool TryGetStandardError(out string standardError)
        {
            if (ErrorLogHandler is ILogHandlerWithCapture errorLogHandlerWithCapture)
            {
                standardError = errorLogHandlerWithCapture.GetCapturedLogMessages();
                return true;
            }

            standardError = null;
            return false;
        }
    }
}
