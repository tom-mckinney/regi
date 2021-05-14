using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface ILogHandlerWithCapture : ILogHandler
    {
        string GetCapturedLogMessages();
    }
}
