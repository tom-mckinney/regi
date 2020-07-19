using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Abstractions
{
    public interface ITopology
    {
        IList<IProject> Projects { get; set; }

        IList<IProject> Services { get; set; }

        IDictionary<ProjectFramework, string> GetSources();
    }
}
