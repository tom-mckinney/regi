using Regi.Abstractions;
using Regi.Abstractions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Docker
{
    public class DockerServiceRunner : IServiceRunner
    {
        public ServiceType Type => ServiceType.Docker;

        public ValueTask<IManagedProcess> RunAsync(IService service, OptionsBase options, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
