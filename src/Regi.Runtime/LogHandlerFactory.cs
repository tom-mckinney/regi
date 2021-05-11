using Microsoft.Extensions.Logging;
using Regi.Abstractions;
using System;

namespace Regi.Runtime
{
    public class LogHandlerFactory : ILogHandlerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public LogHandlerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public TLogHandler CreateLogHandler<TLogHandler>(string serviceName)
            where TLogHandler : LogHandlerBase
        {
            return (TLogHandler)Activator.CreateInstance(typeof(TLogHandler), serviceName, _loggerFactory.CreateLogger<TLogHandler>());
        }
    }
}
