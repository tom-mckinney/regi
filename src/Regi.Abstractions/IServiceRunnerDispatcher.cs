using Regi.Abstractions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface IServiceRunnerDispatcher
    {
        ValueTask<IManagedProcess> DispatchAsync(IService service, OptionsBase options, CancellationToken cancellationToken);
    }
}
