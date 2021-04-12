using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Docker
{
    public interface IDockerService : IService
    {
        string Image { get; set; }
        List<string> Ports { get; set; }
        List<string> Volumes { get; set; }
    }

    public class DockerService : IDockerService
    {
        public string Image { get; set; }
        public List<string> Ports { get; set; }
        public List<string> Volumes { get; set; }
    }
}
