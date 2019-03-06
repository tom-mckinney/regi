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

        public static void WriteIndentedLine(this IConsole console, string input, int indentCount, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                console.ForegroundColor = color.Value;
            }

            console.WriteLine(new string(' ', indentCount * 2) + input);

            if (color.HasValue)
            {
                console.ResetColor();
            }
        }
    }
}
