using System.Collections.Generic;

namespace Regi.Abstractions
{
    public interface IServiceMesh : IServiceMesh<IProject>
    {
    }

    public interface IServiceMesh<TProject> where TProject : IProject
    {
        string Path { get; }

        IList<TProject> Projects { get; set; }

        IList<TProject> Services { get; set; }

        IDictionary<ProjectFramework, string> GetSources();
    }
}
