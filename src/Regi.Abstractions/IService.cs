using System.Collections.Generic;

namespace Regi.Abstractions
{
    public interface IService
    {
        string Name { get; set; }
        ServiceType Type { get; }
    }
}
