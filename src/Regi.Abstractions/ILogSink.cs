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
        DataReceivedEventHandler OutputHandler { get; }
        DataReceivedEventHandler ErrorHandler { get; }
    }
}
