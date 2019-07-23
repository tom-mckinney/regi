using System.Collections.Generic;
using System.Linq;

namespace Regi.Models
{
    public class ProjectOptions : Dictionary<string, IList<string>>
    {
        public void AddOptions(string command, params string[] options)
        {
            if (ContainsKey(command) && this[command] != null)
            {
                foreach(var option in options)
                {
                    this[command].Add(option);
                }
            }
            else
            {
                this[command] = new List<string>(options);
            }
        }

        public bool TryGetValue(AppTask task, out IList<string> options)
        {
            IEnumerable<string> values = new List<string>();

            string key = task.ToString().ToLowerInvariant();

            if (TryGetValue(key, out IList<string> taskOptions))
            {
                values = values.Concat(taskOptions);
            }

            if (TryGetValue("*", out IList<string> anyOptions))
            {
                values = values.Concat(anyOptions);
            }

            options = values.ToList();

            return options.Count > 0;
        }
    }
}
