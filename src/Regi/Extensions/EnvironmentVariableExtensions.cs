using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Regi.Extensions
{
    public static class EnvironmentVariableExtensions
    {
        public static bool TryAdd(this StringDictionary environmentVariables, string key, string value)
        {
            if (environmentVariables.ContainsKey(key))
            {
                return false;
            }

            environmentVariables.Add(key, value);

            return true;
        }
    }
}
