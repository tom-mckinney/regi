using McMaster.Extensions.CommandLineUtils;
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
        public static void KillAll(this IEnumerable<AppProcess> processes)
        {
            KillAll(processes, null);
        }

        public static void KillAll(this IEnumerable<AppProcess> processes, IConsole console)
        {
            if (processes?.Count() > 0)
            {
                foreach (var p in processes)
                {
                    p.Kill(console);
                }
            }
        }
    }
}
