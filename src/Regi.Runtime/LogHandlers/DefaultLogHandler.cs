using Microsoft.Extensions.Logging;
using Regi.Abstractions;

namespace Regi.Runtime.LogHandlers
{
    public class DefaultLogHandler : LogHandlerBase
    {
        public DefaultLogHandler(ILogger logger)
            : base(logger)
        {
        }

        public override void Handle(LogLevel logLevel, string message, params object[] args)
        {
            Logger.Log(logLevel, message, args);
        }
    }
}
