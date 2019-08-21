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
            console.WriteErrorLine(e.Message);

            return 1;
        }

        public static int LogAndReturnStatus(this Exception e, IConsole console)
        {
            string failureMessage = e.InnerException?.Message ?? e.Message;

            console.WriteErrorLine(failureMessage);
            console.WriteLine();
            console.WriteErrorLine($"Stack Trace: {e.StackTrace}");

            return 1;
        }

        public static int LogAndReturnStatus(this JsonSerializationException e, IConsole console)
        {
            console.WriteErrorLine(e.Message);
            console.WriteLine();
            console.WriteErrorLine($"Stack Trace: {e.StackTrace}");

            return 1;
        }
    }
}
