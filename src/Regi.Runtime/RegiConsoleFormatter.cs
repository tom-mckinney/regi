using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System.IO;

namespace Regi.Runtime
{
    public class RegiConsoleFormatterOptions : ConsoleFormatterOptions
    {
    }

    public class RegiConsoleFormatter : ConsoleFormatter
    {
        public RegiConsoleFormatter() : base("Regi")
        {
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

            if (message == null)
            {
                return;
            }

            if (message.EndsWith(textWriter.NewLine))
            {
                textWriter.Write(message);
            }
            else
            {
                textWriter.WriteLine(message);
            }
        }
    }
}
