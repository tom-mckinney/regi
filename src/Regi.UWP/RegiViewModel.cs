using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.UWP
{
    public class Project
    {
        public string Name { get; set; }

        public string Framework { get; set; }
    }

    public class RegiViewModel
    {
        public ObservableCollection<Project> Projects { get; } = new ObservableCollection<Project>
        {
            new Project { Name = "Regi", Framework = "dotnet" },
            new Project { Name = "Upstream.Cms", Framework = "dotnet" },
            new Project { Name = "Upstream.Testing", Framework = "dotnet" },
        };
    }
}
