using Microsoft.Extensions.Logging;
using Regi.Abstractions;

namespace Regi.Runtime.LogHandlers
{
    public class SilentLogHandler : LogHandlerBase
    {
        public SilentLogHandler(ILogger logger)
            : base(logger)
        {
        }

        public override void Handle(LogLevel logLevel, string message, params object[] args)
        {
            // silence is a virtue
        }
    }
}
