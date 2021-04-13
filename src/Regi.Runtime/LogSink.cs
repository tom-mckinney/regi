using Regi.Abstractions;
using System;
using System.Diagnostics;

namespace Regi.Runtime
{
    public class LogSink : ILogSink
    {
        public Guid ManagedProcessId { get; internal set; }

        public DataReceivedEventHandler OutputHandler { get; internal set; }

        public DataReceivedEventHandler ErrorHandler { get; internal set; }
    }
}
