using Regi.Abstractions;
using Regi.Runtime.LogHandlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Runtime
{
    public class LogSinkManager : ILogSinkManager
    {
        private readonly ILogHandlerFactory _logHandlerFactory;

        public LogSinkManager(ILogHandlerFactory logHandlerFactory)
        {
            _logHandlerFactory = logHandlerFactory;
        }

        internal ConcurrentDictionary<Guid, ILogSink> LogSinks { get; } = new ConcurrentDictionary<Guid, ILogSink>();

        public ValueTask<ILogSink> CreateAsync(Guid managedProcessId)
        {
            var logSink = new LogSink(
                managedProcessId,
                _logHandlerFactory.CreateLogHandler<DefaultLogHandler>(), // TODO: make this variable
                _logHandlerFactory.CreateLogHandler<DefaultLogHandler>());

            if (!LogSinks.TryAdd(managedProcessId, logSink))
            {
                throw new InvalidOperationException($"LogSink already registered with managed process ID {managedProcessId}");
            }

            return new ValueTask<ILogSink>(logSink);
        }
    }
}
