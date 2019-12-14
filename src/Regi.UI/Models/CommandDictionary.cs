using System.Collections.Generic;
using System.Linq;

namespace Regi.Models
{
    public class CommandDictionary<TValue> : Dictionary<string, IList<TValue>>
    {
        public void AddOptions(string command, params TValue[] options)
        {
            if (ContainsKey(command) && this[command] != null)
            {
                foreach (var option in options)
                {
                    this[command].Add(option);
                }
            }
            else
            {
                this[command] = new List<TValue>(options);
            }
        }

        public bool TryGetValue(AppTask task, out IList<TValue> options)
        {
            IEnumerable<TValue> values = new List<TValue>();

            string key = task.ToString().ToLowerInvariant();

            if (TryGetValue(key, out IList<TValue> taskOptions))
            {
                values = values.Concat(taskOptions);
            }

            if (TryGetValue("*", out IList<TValue> anyOptions))
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
