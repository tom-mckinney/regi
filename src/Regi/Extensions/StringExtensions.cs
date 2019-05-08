using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regi.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts string to all capital letters with underscores instead of spaces.
        /// Example: Test Project 1 ---> TEST_PROJECT_1
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns></returns>
        public static string ToUnderscoreCase(this string str)
        {
            return string.Concat(str.Select((x, i) => char.IsWhiteSpace(x) || char.IsPunctuation(x) ? "_" : x.ToString())).ToUpperInvariant();
        }
    }
}
