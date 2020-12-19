using System.Collections.Generic;

namespace Regi.Abstractions
{
    public interface IServiceMesh
    {
        IList<IProject> Projects { get; set; }

        IList<IProject> Services { get; set; }

        IDictionary<ProjectFramework, string> GetSources();
    }
}
