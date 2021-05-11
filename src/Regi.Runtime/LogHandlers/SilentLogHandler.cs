using Microsoft.Extensions.Logging;
using Regi.Abstractions;

namespace Regi.Runtime.LogHandlers
{
    public class SilentLogHandler : LogHandlerBase
    {
        public SilentLogHandler(string serviceName, ILogger logger)
            : base(serviceName, logger)
        {
        }

        public override void Handle(LogLevel logLevel, string message, params object[] args)
        {
            // silence is a virtue
        }
    }
}
