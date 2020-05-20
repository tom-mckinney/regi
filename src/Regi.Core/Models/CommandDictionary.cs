using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Models
{
    public class CommandDictionary<TValue> : Dictionary<string, ICollection<TValue>>
    {
        public CommandDictionary() : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }

        public bool TryGetValue(AppTask task, out ICollection<TValue> options)
        {
            IEnumerable<TValue> values = new List<TValue>();

            string key = task.ToString();

            if (TryGetValue(key, out ICollection<TValue> taskOptions))
            {
                values = values.Concat(taskOptions);
            }

            if (TryGetValue("*", out ICollection<TValue> anyOptions))
            {
                values = values.Concat(anyOptions);
            }

            options = values.ToList();

            return options.Count > 0;
        }
    }

    public class CommandDictionary : CommandDictionary<string>
    {
    }
}
