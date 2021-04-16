using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Regi.Models
{
    public class ServiceMesh : ServiceMesh<IProject, IServiceMultiplexer>, IServiceMesh
    {
    }

    public class ServiceMesh<TProject, TService> : IServiceMesh<TProject, TService>
        where TProject : IProject
        where TService : IService
    {
        [JsonIgnore]
        public string Path { get; set; }

        [JsonPropertyName("projects")]
        public IList<TProject> Projects { get; set; } = new List<TProject>();

        [JsonPropertyName("services")]
        public IList<TService> Services { get; set; } = new List<TService>();

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

        public static explicit operator ServiceMesh(ServiceMesh<TProject> input)
        {
            return new ServiceMesh
            {
                Path = input.Path,
                Projects = input.Projects.Select(p => (IProject)p).ToList(),
                Services = input.Services.Select(p => (IProject)p).ToList(),
                RawSources = input.RawSources
            };
        }
    }
}
