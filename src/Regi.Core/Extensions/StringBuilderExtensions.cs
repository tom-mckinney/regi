using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Extensions
{
    public static class StringBuilderExtensions
    {
        private static readonly object _lock = new object();

        public static StringBuilder AppendCliOption(this StringBuilder builder, string option)
        {
            lock (_lock)
            {
                return builder.Append(' ').Append(option);
            }
        }

        public static StringBuilder AppendJoinCliOptions(this StringBuilder builder, IEnumerable<string> options)
        {
            lock (_lock)
            {
                return builder.Append(' ').AppendJoin(' ', options);
            }
        }
    }
}
