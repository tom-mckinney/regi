using Microsoft.Extensions.Logging;

namespace Regi.Abstractions
{
    public interface ILogHandler
    {
        void Handle(LogLevel logLevel, string message, params object[] args);
    }
}
