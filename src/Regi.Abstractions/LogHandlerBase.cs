using Microsoft.Extensions.Logging;

namespace Regi.Abstractions
{
    public abstract class LogHandlerBase : ILogHandler
    {
        protected LogHandlerBase(ILogger logger)
        {
            Logger = logger;
        }

        protected virtual ILogger Logger { get; set; }

        public abstract void Handle(LogLevel logLevel, string message, params object[] args);
    }
}
