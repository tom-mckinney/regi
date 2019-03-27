using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void WriteSuccessLine(this IConsole console, string input)
        {
            console.ForegroundColor = ConsoleColor.Green;

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

            console.WriteLine(Indent(indentCount) + input);

            if (color.HasValue)
            {
                console.ResetColor();
            }
        }

        public static void WritePropertyIfSpecified(this IConsole console, string propertyName, ProjectOptions propertyValue, int indentCount = 2)
        {
            if (propertyValue?.Count > 0)
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendLine("[");

                foreach (var (option, index) in propertyValue.WithIndex())
                {
                    builder.Append(Indent(indentCount + 1))
                        .Append("[")
                        .Append(option.Key)
                        .Append(", [")
                        .AppendJoin(", ", option.Value.Select(v => '"' + v + '"'))
                        .Append("]]");

                    if (index != propertyValue.Count - 1)
                        builder.AppendLine(",");
                    else
                        builder.AppendLine("");
                }

                builder.Append(Indent(indentCount))
                    .Append("]");

                WritePropertyIfSpecified(console, propertyName, builder.ToString(), indentCount);
            }
        }

        public static void WritePropertyIfSpecified<T>(this IConsole console, string propertyName, ICollection<T> propertyValue, int indentCount = 2)
        {
            if (propertyValue?.Count > 0)
            {
                WritePropertyIfSpecified(console, propertyName, $"[{string.Join(", ", propertyValue)}]", indentCount);
            }
        }

        public static void WritePropertyIfSpecified(this IConsole console, string propertyName, object propertyValue, int indentCount = 2)
        {
            if (propertyValue == null || propertyValue is string propertyValueString && string.IsNullOrWhiteSpace(propertyValueString))
                return;

            console.WriteIndentedLine($"{propertyName}: {propertyValue}", indentCount, ConsoleColor.DarkGreen);
        }

        private static string Indent(int indentCount) => new string(' ', indentCount * 2);
    }
}
