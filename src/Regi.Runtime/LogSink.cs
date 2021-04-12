using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Runtime
{
    public class LogSink : ILogSink
    {
        public DataReceivedEventHandler OutputHandler => throw new NotImplementedException();

        public DataReceivedEventHandler ErrorHandler => throw new NotImplementedException();
    }
}
