using Regi.Abstractions;
using Regi.Abstractions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Runtime
{
    public class ServiceRunnerDispatcher : IServiceRunnerDispatcher
    {
        private readonly IEnumerable<IServiceRunner> _serviceRunners;

        public ServiceRunnerDispatcher(IEnumerable<IServiceRunner> serviceRunners)
        {
            _serviceRunners = serviceRunners;
        }

        public ValueTask<IManagedProcess> DispatchAsync(IServiceOmnibus service, OptionsBase options, CancellationToken cancellationToken)
        {
            var serviceRunner = _serviceRunners.FirstOrDefault(r => r.Type == service.Type);

            if (serviceRunner == null)
            {
                throw new NotImplementedException($"Regi does not support {service.Type} services. Received {_serviceRunners.Count()} service runners");
            }

            return serviceRunner.RunAsync(service, options, cancellationToken);
        }
    }
}
