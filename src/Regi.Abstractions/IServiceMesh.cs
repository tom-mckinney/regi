using System.Collections.Generic;

namespace Regi.Abstractions
{
    public interface IServiceMesh : IServiceMesh<IProject, IServiceMultiplexer>
    {
    }

    public interface IServiceMesh<TProject, TService>
        where TProject : IProject
        where TService : IService
    {
        string Path { get; }

        IList<TProject> Projects { get; set; }

        IList<TService> Services { get; set; }

        IDictionary<ProjectFramework, string> GetSources();
    }
}
