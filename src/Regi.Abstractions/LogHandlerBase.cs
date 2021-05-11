using Microsoft.Extensions.Logging;

namespace Regi.Abstractions
{
    public abstract class LogHandlerBase : ILogHandler
    {
        protected LogHandlerBase(string serviceName, ILogger logger)
        {
            ServiceName = serviceName;
            Logger = logger;
        }

        public string ServiceName { get; protected set; }

        protected virtual ILogger Logger { get; set; }

        public abstract void Handle(LogLevel logLevel, string message, params object[] args);
    }
}
