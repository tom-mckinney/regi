using Regi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.UI
{
    public class Project
    {
        public string Name { get; set; }
    }

    public class RegiViewModel
    {
        public ObservableCollection<Project> Projects { get; } = new ObservableCollection<Project>
        {
            new Project { Name = "Regi" },
            new Project { Name = "Upstream.Cms" },
            new Project { Name = "Upstream.Testing" },
        };
    }
}
