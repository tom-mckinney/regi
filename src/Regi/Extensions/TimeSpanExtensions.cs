using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToHumanFriendlyString(this TimeSpan t)
        {
            if (t.TotalSeconds <= 1)
            {
                return $@"{t.Milliseconds}ms";
            }
            if (t.TotalMinutes <= 1)
            {
                return $@"{t.Seconds}s {t.Milliseconds}ms";
            }
            if (t.TotalHours <= 1)
            {
                return $@"{t.Minutes}m {t.Seconds}s {t.Milliseconds}ms";
            }
            if (t.TotalDays <= 1)
            {
                return $@"{t.Hours}h {t.Minutes}m {t.Seconds}s {t.Milliseconds}ms";
            }

            return $@"{t.Days} days {t.Hours}h {t.Minutes}m {t.Seconds}s {t.Milliseconds}ms";
        }
    }
}
