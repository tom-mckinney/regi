using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Runtime
{
    public class ServiceMultiplexer : IServiceMultiplexer
    {
        public string Name { get; set; }

        public ServiceType Type { get; set; }

        // Docker
        public string Image { get; set; }
        public List<string> Ports { get; set; }
        public List<string> Volumes { get; set; }
    }
}
