using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Serialization
{
    public interface IServiceMeshSerializer
    {
        ValueTask<IServiceMesh> 
    }

    public class ServiceMeshSerializer : IServiceMeshSerializer
    {
    }
}
