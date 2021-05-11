using Microsoft.Extensions.Logging;
using Regi.Abstractions;

namespace Regi.Runtime.LogHandlers
{
    public class DefaultLogHandler : LogHandlerBase
    {
        public DefaultLogHandler(string serviceName, ILogger logger)
            : base(serviceName, logger)
        {
        }

        public override void Handle(LogLevel logLevel, string message, params object[] args)
        {
            Logger.Log(logLevel, message, args);
        }
    }
}
