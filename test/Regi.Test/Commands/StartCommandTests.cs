using Microsoft.Extensions.Options;
using Moq;
using Regi.Abstractions;
using Regi.Abstractions.Options;
using Regi.CommandLine.Commands;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using Regi.Test.Stubs;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Commands
{
    public class StartCommandTests
    {
        private readonly TestConsole _console;
        private readonly IProjectManager _projectManager;
        private readonly Mock<IRunnerService> _runnerServiceMock = new(MockBehavior.Strict);
        private readonly Mock<IServiceRunnerDispatcher> _serviceRunnerDispatcherMock = new(MockBehavior.Strict);
        private readonly Mock<ICleanupService> _cleanupServiceMock = new(MockBehavior.Strict);
        private readonly Mock<IConfigurationService> _configServiceMock = new(MockBehavior.Strict);
        private readonly IOptions<Settings> _options = Options.Create(new Settings { RunIndefinitely = false });

        public StartCommandTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
            _projectManager = new ProjectManager(_console, _cleanupServiceMock.Object, new ProjectFilter());
        }

        StartCommand CreateCommand()
        {
            return new StartCommand(
                _runnerServiceMock.Object,
                _serviceRunnerDispatcherMock.Object,
                _projectManager,
                _configServiceMock.Object,
                _console,
                _options);
        }

        [Fact]
        public async Task Will_start_all_projects_by_default()
        {
            _configServiceMock.Setup(m => m.GetConfigurationAsync(It.IsAny<CommandOptions>()))
                .ReturnsAsync(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _runnerServiceMock.Setup(m => m.StartAsync(It.IsAny<IList<IProject>>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .Callback((IList<IProject> projects, CommandOptions options, CancellationToken token) =>
                {
                    foreach (var p in projects)
                    {
                        p.Processes.Add(new AppProcess(new Process(), AppTask.Start, AppStatus.Success));
                    }
                })
                .Returns((IList<IProject> projects, CommandOptions options, CancellationToken token) => Task.FromResult(projects))
                .Verifiable();

            StartCommand command = CreateCommand();

            int statusCode = await command.OnExecute();

            Assert.Equal(0, statusCode);

            _configServiceMock.VerifyAll();
            _runnerServiceMock.VerifyAll();
            _serviceRunnerDispatcherMock.VerifyAll();
        }

        [Fact]
        public async Task Will_start_services_if_present()
        {
            var serviceMesh = SampleProjects.ConfigurationWithServices;

            _configServiceMock.Setup(m => m.GetConfigurationAsync(It.IsAny<CommandOptions>()))
                .ReturnsAsync(serviceMesh);
            _runnerServiceMock.Setup(m => m.StartAsync(It.IsAny<IList<IProject>>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .Callback((IList<IProject> projects, CommandOptions options, CancellationToken token) =>
                {
                    foreach (var p in projects)
                    {
                        p.Processes.Add(new AppProcess(new Process(), AppTask.Start, AppStatus.Success));
                    }
                })
                .Returns((IList<IProject> projects, CommandOptions options, CancellationToken token) => Task.FromResult(projects));

            _serviceRunnerDispatcherMock.Setup(m => m.DispatchAsync(serviceMesh.Services[0], It.IsAny<OptionsBase>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StubbedManagedProcess());

            StartCommand command = CreateCommand();

            int statusCode = await command.OnExecute();

            Assert.Equal(0, statusCode);

            _configServiceMock.VerifyAll();
            _runnerServiceMock.VerifyAll();
            _serviceRunnerDispatcherMock.VerifyAll();
        }
    }
}
