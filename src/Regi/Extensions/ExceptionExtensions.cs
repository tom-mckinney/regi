using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Regi.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Extensions
{
    public static class ExceptionExtensions
    {
        public static int LogAndReturnStatus(this RegiException e, IConsole console)
        {
            LogMessage(e, console);

            return 1;
        }

        public static int LogAndReturnStatus(this CommandParsingException e, IConsole console)
        {
            LogMessage(e, console);

            return 1;
        }

        public static int LogAndReturnStatus(this Exception e, IConsole console)
        {
            LogMessageAndDetails(e, console);

            return 1;
        }

        public static int LogAndReturnStatus(this JsonSerializationException e, IConsole console)
        {
            LogMessageAndDetails(e, console);

            return 1;
        }

        private static void LogMessage(Exception e, IConsole console)
        {
            string message = e.InnerException?.Message ?? e.Message;

            console.WriteErrorLine(message);
        }

        private static void LogMessageAndDetails(Exception e, IConsole console)
        {
            string message = e.InnerException?.Message ?? e.Message;

            console.WriteErrorLine(message);
            console.WriteLine();
            console.WriteErrorLine($"Type: {e.GetType().Name}");
            console.WriteErrorLine($"Stack Trace: {e.StackTrace}");
        }
    }
}
