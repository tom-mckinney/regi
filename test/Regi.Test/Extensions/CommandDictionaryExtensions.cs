using Regi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Test.Extensions
{
    public static class CommandDictionaryExtensions
    {
        public static void AddOptions<TValue>(this CommandDictionary<TValue> dictionary, string command, params TValue[] options)
        {
            if (dictionary.ContainsKey(command) && dictionary[command] != null)
            {
                foreach (var option in options)
                {
                    dictionary[command].Add(option);
                }
            }
            else
            {
                dictionary[command] = new List<TValue>(options);
            }
        }
    }
}
