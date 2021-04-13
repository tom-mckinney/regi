using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface ILogSink
    {
        Guid ManagedProcessId { get; }

        ILogHandler OutputLogHandler { get; set; }
        ILogHandler ErrorLogHandler { get; set; }

        DataReceivedEventHandler OutputEventHandler { get; }
        DataReceivedEventHandler ErrorEventHandler { get; }
    }
}
