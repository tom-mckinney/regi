using Regi.Abstractions;
using Regi.Abstractions.Options;
using Regi.Abstractions.Services;
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
        private readonly IProcessManager _processManager;

        public DockerServiceRunner(IProcessManager processManager)
        {
            _processManager = processManager;
        }

        public ServiceType Type => ServiceType.Docker;

        public async ValueTask<IManagedProcess> RunAsync(IService service, OptionsBase options, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debugger.Launch();
            if (service is not IDockerService dockerService) // todo: user generic runners
            {
                throw new NotImplementedException();
            }

            var builder = new CommandBuilder();

            builder.Add("run");
            builder.Add("--name", dockerService.Name);
            builder.Add("-p", dockerService.Ports);
            builder.Add("-v", dockerService.Volumes);
            builder.Add(dockerService.Image);

            var process = await _processManager.CreateAsync("docker", builder.Build());

            await process.StartAsync(cancellationToken);

            return process;
        }
    }
}
