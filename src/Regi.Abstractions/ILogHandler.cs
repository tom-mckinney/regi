using Microsoft.Extensions.Logging;

namespace Regi.Abstractions
{
    public interface ILogHandler
    {
        string ServiceName { get; }
        void Handle(LogLevel logLevel, string message, params object[] args);
    }
}
