using McMaster.Extensions.CommandLineUtils;
using System;

namespace Regi.Extensions
{
    public static class ExceptionExtensions
    {
        public static int LogAndReturnStatus(this Exception e, IConsole console)
        {
            console.WriteErrorLine(e.Message);

            return 1;
        }

        public static int LogAllDetailsAndReturnStatus(this Exception e, IConsole console)
        {
            string failureMessage = e.InnerException?.Message ?? e.Message;

            console.WriteErrorLine(failureMessage);
            console.WriteLine();
            console.WriteErrorLine($"Stack Trace: {e.StackTrace}");

            return 1;
        }
    }
}
