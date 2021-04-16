using Regi.Abstractions.Services;

namespace Regi.Abstractions
{
    /// <summary>
    /// Implements every <see cref="IService"/>. Used for deserialization since covariant types are not supported yet.
    /// </summary>
    public interface IServiceMultiplexer
        : IDockerService
    {
    }
}
