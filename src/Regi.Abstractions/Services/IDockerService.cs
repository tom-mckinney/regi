using System.Collections.Generic;

namespace Regi.Abstractions.Services
{
    public interface IDockerService : IService
    {
        string Image { get; set; }
        List<string> Ports { get; set; }
        List<string> Volumes { get; set; }
    }
}
