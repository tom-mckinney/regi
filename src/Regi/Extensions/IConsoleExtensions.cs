using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Extensions
{
    public static class IConsoleExtensions
    {
        public static void WriteEmphasizedLine(this IConsole console, string input)
        {
            console.ForegroundColor = ConsoleColor.Cyan;

            console.WriteLine(input);

            console.ResetColor();
        }

        public static void WriteErrorLine(this IConsole console, string input)
        {
            console.ForegroundColor = ConsoleColor.Red;

            console.WriteLine(input);

            console.ResetColor();
        }
    }
}
