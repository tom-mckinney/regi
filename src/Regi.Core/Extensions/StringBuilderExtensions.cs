using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendCliOption(this StringBuilder builder, string option)
        {
            return builder.Append(' ').Append(option);
        }

        public static StringBuilder AppendJoinCliOptions(this StringBuilder builder, IEnumerable<string> options)
        {
            return builder.Append(' ').AppendJoin(' ', options);
        }
    }
}
