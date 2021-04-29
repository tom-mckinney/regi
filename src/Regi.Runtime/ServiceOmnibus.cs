using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Runtime
{
    public class ServiceOmnibus : IServiceOmnibus
    {
        // Common
        public string Name { get; set; }
        public ServiceType Type { get; set; }
        public IDictionary<string, object> Environment { get; set; } = new Dictionary<string, object>();

        // Docker
        public string Image { get; set; }
        public List<string> Ports { get; set; }
        public List<string> Volumes { get; set; }
    }
}
