using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regi.Extensions
{
    public enum ConsoleLogLevel
    {
        Default = 0,
        Error,
        Warning
    }

    public enum ConsoleLineStyle
    {
        Normal = 0,
        LineBefore,
        LineAfter,
        LineBeforeAndAfter
    }

    public static class ConsoleExtensions
    {
        private static readonly object _consoleLock = new object();

        public static void WriteDefaultLine(this IConsole console, string input, ConsoleLineStyle style = ConsoleLineStyle.Normal)
        {
            lock (_consoleLock)
            {
                console.ResetColor();

                WriteLineWithStyle(console, input, style);
            }
        }

        public static void WriteEmphasizedLine(this IConsole console, string input, ConsoleLineStyle style = ConsoleLineStyle.Normal)
        {
            lock (_consoleLock)
            {
                console.ForegroundColor = ConsoleColor.Cyan;

                WriteLineWithStyle(console, input, style);

                console.ResetColor();
            }
        }

        public static void WriteSuccessLine(this IConsole console, string input, ConsoleLineStyle style = ConsoleLineStyle.Normal)
        {
            lock (_consoleLock)
            {
                console.ForegroundColor = ConsoleColor.Green;

                WriteLineWithStyle(console, input, style);

                console.ResetColor();
            }
        }

        public static void WriteErrorLine(this IConsole console, string input, ConsoleLineStyle style = ConsoleLineStyle.Normal)
        {
            lock (_consoleLock)
            {
                console.ForegroundColor = ConsoleColor.Red;

                WriteLineWithStyle(console, input, style);

                console.ResetColor();
            }
        }

        public static void WriteWarningLine(this IConsole console, string input, ConsoleLineStyle style = ConsoleLineStyle.Normal)
        {
            lock (_consoleLock)
            {
                console.ForegroundColor = ConsoleColor.Yellow;

                WriteLineWithStyle(console, input, style);

                console.ResetColor();
            }
        }

        public static void WriteIndentedLine(this IConsole console, string input, int indentCount, ConsoleColor? color = null)
        {
            lock (_consoleLock)
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

        public static void WritePropertyIfSpecified<T>(this IConsole console, string propertyName, ICollection<T> propertyValue, bool linebreakItems = false, int indentCount = 2)
        {
            if (propertyValue?.Count > 0)
            {
                if (linebreakItems)
                {
                    StringBuilder builder = new StringBuilder();

                    builder.AppendLine("[");

                    for (int i = 0; i < propertyValue.Count; i++)
                    {
                        var value = propertyValue.ElementAt(0);

                        builder.Append(Indent(indentCount + 1))
                            .Append(value);

                        if (i < propertyValue.Count - 1)
                            builder.AppendLine(",");
                        else
                            builder.AppendLine();
                    }

                    builder.Append(Indent(indentCount))
                        .Append("]");

                    WritePropertyIfSpecified(console, propertyName, builder.ToString(), indentCount);
                }
                else
                {
                    WritePropertyIfSpecified(console, propertyName, $"[{string.Join(", ", propertyValue)}]", indentCount);
                }
            }
        }

        public static void WritePropertyIfSpecified(this IConsole console, string propertyName, bool propertyValue, int indentCount = 2)
        {
            if (!propertyValue)
                return;

            console.WriteIndentedLine($"- {propertyName}: {propertyValue}", indentCount);
        }

        public static void WritePropertyIfSpecified(this IConsole console, string propertyName, ProjectType propertyValue, int indentCount = 2)
        {
            if (propertyValue == ProjectType.Unknown)
                return;

            console.WriteIndentedLine($"- {propertyName}: {propertyValue}", indentCount);
        }

        public static void WritePropertyIfSpecified(this IConsole console, string propertyName, object propertyValue, int indentCount = 2)
        {
            if (propertyValue == null || propertyValue is string propertyValueString && string.IsNullOrWhiteSpace(propertyValueString))
                return;

            console.WriteIndentedLine($"- {propertyName}: {propertyValue}", indentCount);
        }

        private static string Indent(int indentCount) => new string(' ', indentCount * 2);

        private static void WriteLineWithStyle(IConsole console, string input, ConsoleLineStyle style)
        {
            lock (_consoleLock)
            {
                switch (style)
                {
                    case ConsoleLineStyle.LineBefore:
                    case ConsoleLineStyle.LineBeforeAndAfter:
                        console.WriteLine();
                        break;
                    case ConsoleLineStyle.LineAfter:
                    case ConsoleLineStyle.Normal:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(style), style, null);
                }

                console.WriteLine(input);

                switch (style)
                {
                    case ConsoleLineStyle.LineAfter:
                    case ConsoleLineStyle.LineBeforeAndAfter:
                        console.WriteLine();
                        break;
                    case ConsoleLineStyle.LineBefore:
                    case ConsoleLineStyle.Normal:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(style), style, null);
                }
            }
        }
    }
}
