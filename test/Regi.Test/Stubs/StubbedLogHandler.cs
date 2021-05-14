using Microsoft.Extensions.Logging;
using Regi.Abstractions;
using System.Text;

namespace Regi.Test.Stubs
{
    public class StubbedLogHandler : ILogHandlerWithCapture
    {
        private readonly StringBuilder _stringBuilder = new();
        private readonly ILogger _logger;

        public StubbedLogHandler(string serviceName, ILogger logger)
        {
            ServiceName = serviceName;
            _logger = logger;
        }

        public string ServiceName { get; set; }


        public void Handle(LogLevel logLevel, string message, params object[] args)
        {
            _stringBuilder.AppendLine(message);

            _logger.Log(logLevel, message, args);
        }

        public string GetCapturedLogMessages()
        {
            return _stringBuilder.ToString();
        }
    }
}
