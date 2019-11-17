using System;
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
                };
                options.Converters.Add(new JsonStringEnumConverter());
                return options;
            }
        }
    }
}
