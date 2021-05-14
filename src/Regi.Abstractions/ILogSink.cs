using System;
using System.Diagnostics;

namespace Regi.Abstractions
{
    public interface ILogSink
    {
        Guid ManagedProcessId { get; }

        ILogHandler OutputLogHandler { get; set; }
        ILogHandler ErrorLogHandler { get; set; }

        DataReceivedEventHandler OutputEventHandler { get; }
        DataReceivedEventHandler ErrorEventHandler { get; }

        bool TryGetStandardOutput(out string standardOutput);
        bool TryGetStandardError(out string standardError);
    }
}
