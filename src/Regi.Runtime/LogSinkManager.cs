using Regi.Abstractions;
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
        internal ConcurrentDictionary<Guid, ILogSink> LogSinks { get; } = new ConcurrentDictionary<Guid, ILogSink>();

        public ValueTask<ILogSink> CreateAsync(Guid managedProcessId)
        {
            var logSink = new LogSink
            {
                ManagedProcessId = managedProcessId
            };

            if (!LogSinks.TryAdd(managedProcessId, logSink))
            {
                throw new InvalidOperationException($"LogSink already registered with managed process ID {managedProcessId}");
            }

            return new ValueTask<ILogSink>(logSink);
        }
    }
}
