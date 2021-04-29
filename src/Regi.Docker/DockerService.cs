using Regi.Abstractions;
using Regi.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Docker
{
    public class DockerService : IDockerService
    {
        public string Name { get; set; }
        public ServiceType Type => ServiceType.Docker;

        public string Image { get; set; }
        public List<string> Ports { get; set; }
        public List<string> Volumes { get; set; }
        public IDictionary<string, object> Environment { get; set; } = new Dictionary<string, object>();
    }
}
