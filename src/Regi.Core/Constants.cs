using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Regi
{
    public static class Constants
    {
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

        public static JsonSerializerOptions DefaultSerializerOptions
        {
            get
            {
                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                };
                options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
                return options;
            }
        }

        public static readonly HashSet<string> DiscoverIgnore = new HashSet<string>
        {
            ".git",
            ".vs",
            "bin",
            "node_modules",
            "obj",
        };
    }
}
