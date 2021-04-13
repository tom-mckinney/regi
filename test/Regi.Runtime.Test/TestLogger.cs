using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Regi.Runtime.Test
{
    public class TestLogEntry
    {
        public string Scope { get; set; }
        public LogLevel LogLevel { get; set; }
        public EventId EventId { get; set; }
        public string Message { get; set; }
    }

    public class TestLogger<T> : TestLogger, ILogger<T>
    {
        public TestLogger() : base(nameof(T))
        {
        }

        public TestLogger(ITestOutputHelper output) : base(output, nameof(T))
        {
        }
    }

    public class TestLogger : ILogger
    {
        private readonly ITestOutputHelper _output;
        private LogLevel _logLevel;

        public TestLogger(string category = "DefaultCategory")
            : this(null, category)
        {
        }

        public TestLogger(ITestOutputHelper output, string category = "DefaultCategory")
        {
            _output = output;
            _logLevel = LogLevel.Information;
            CurrentScope = category;
        }

        public bool HasEntries => Entries.Count > 0;

        public List<TestLogEntry> Entries = new();

        public string CurrentScope { get; private set; }

        public void SetLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _logLevel;
        }

        public bool HasEntriesAtLevels(params LogLevel[] otherLevels)
        {
            foreach (LogLevel otherlevel in otherLevels)
            {
                if (!Entries.Any((e) => e.LogLevel == otherlevel))
                {
                    return false;
                }
            }

            return true;
        }

        public void Clear()
        {
            Entries.Clear();
        }

        public bool HasEntriesOnlyAtLevels(params LogLevel[] otherLevels)
        {
            return !Entries.Any((e => !otherLevels.Contains(e.LogLevel)));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            CurrentScope = $"{CurrentScope} > {state}";

            return new TestLoggingScope(this, state);
        }

        public void EndScope<TState>(TState state)
        {
            CurrentScope = CurrentScope.Remove(CurrentScope.Length - (state.ToString().Length + 3));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);

            Entries.Add(new TestLogEntry
            {
                Scope = CurrentScope,
                LogLevel = logLevel,
                EventId = eventId,
                Message = message
            });

            if (_output != null)
            {
                try
                {
                    _output.WriteLine(message);
                }
                catch
                {
                    // xunit test has concluded so the ITestOutputHelper is invalid
                }
            }
        }
    }

    internal class TestLoggingScope : IDisposable
    {
        private readonly object _state;
        private readonly TestLogger _testLogger;

        public TestLoggingScope(TestLogger testLogger, object state)
        {
            _testLogger = testLogger;
            _state = state;
        }

        public void Dispose()
        {
            _testLogger.EndScope(_state);
        }
    }
}
