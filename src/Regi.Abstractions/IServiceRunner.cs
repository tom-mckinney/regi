using Regi.Abstractions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface IServiceRunner
    {
        ServiceType Type { get; }

        ValueTask<IManagedProcess> RunAsync(IService service, OptionsBase options, CancellationToken cancellationToken);
    }
}
