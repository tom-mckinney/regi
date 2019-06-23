using Regi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Regi.Extensions
{
    public static class ListExtensions
    {
        public static void DisposeAll(this IEnumerable<AppProcess> processes)
        {
            if (processes?.Count() > 0)
            {
                foreach (var p in processes)
                {
                    p.Dispose();
                }
            }
        }
    }
}
