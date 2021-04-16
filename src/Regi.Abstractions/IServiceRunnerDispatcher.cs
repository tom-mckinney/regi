using Regi.Abstractions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface IServiceRunnerDispatcher
    {
        ValueTask<IManagedProcess> DispatchAsync(IServiceMultiplexer service, OptionsBase options, CancellationToken cancellationToken);
    }
}
