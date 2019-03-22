using System.Collections.Generic;

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
    }
}
