using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Regi.Models
{
    public class RegiConfig : IServiceMesh
    {
        [JsonIgnore]
        public string Path { get; set; }

        [JsonPropertyName("projects")]
        public IList<IProject> Projects { get; set; } = new List<IProject>();

        // TODO: Delete this property when Regi 1.0.0 is release
        [Obsolete("This is temporary for interactive documentation. Use Projects instead")]
        [JsonPropertyName("apps")]
        public List<object> Apps
        {
            get => null;
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            set => throw new RegiException("The properties \"apps\" and \"tests\" have been removed. Use \"projects\" instead.");
        }

        // TODO: Delete this property when Regi 1.0.0 is release
        [Obsolete("This is temporary for interactive documentation. Use Projects instead")]
        [JsonPropertyName("tests")]
        public List<object> Tests
        {
            get => null;
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            set => throw new RegiException("The properties \"apps\" and \"tests\" have been removed. Use \"projects\" instead.");
        }

        [JsonPropertyName("services")]
        public IList<IProject> Services { get; set; } = new List<IProject>();

        // TODO: Remove RawSources when .NET 5 supports deserializing all Dictionary keys
        [JsonPropertyName("sources")]
        public IDictionary<string, string> RawSources { get; set; } = new Dictionary<string, string>();

        private IDictionary<ProjectFramework, string> _sources;
        public IDictionary<ProjectFramework, string> GetSources()
        {
            if (_sources == null)
            {
                _sources = new Dictionary<ProjectFramework, string>();

                foreach (var source in RawSources)
                {
                    try
                    {
                        var key = Enum.Parse<ProjectFramework>(source.Key, true);
                            
                        _sources.Add(key, source.Value);
                    }
                    catch (ArgumentException)
                    {
                        var projectFrameworkValues = string.Join(", ", typeof(ProjectFramework).GetEnumNames());

                        throw new RegiException($"Recieved invalid source key:'{source.Key}'. Must be one of the following: {projectFrameworkValues}");
                    }
                }
            }

            return _sources;
        }
    }
}
