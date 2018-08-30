using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Extensions
{
    public static class IConsoleExtensions
    {
        public static IConsole WriteErrorLine(this IConsole console, string input)
        {
            console.BackgroundColor = ConsoleColor.DarkRed;
            console.ForegroundColor = ConsoleColor.White;

            console.WriteLine(input);

            console.ResetColor();

            return console;
        }
    }
}
