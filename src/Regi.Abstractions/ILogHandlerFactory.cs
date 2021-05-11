using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface ILogHandlerFactory
    {
        TLogHandler CreateLogHandler<TLogHandler>(string serviceName) where TLogHandler : LogHandlerBase;
    }
}
