using Microsoft.Extensions.Options;
using Moq;
using Regi.CommandLine.Commands;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
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
        private readonly Mock<ICleanupService> _cleanupServiceMock = new Mock<ICleanupService>();
        private readonly Mock<IRunnerService> _runnerServiceMock = new Mock<IRunnerService>();
        private readonly Mock<IConfigurationService> _configServiceMock = new Mock<IConfigurationService>();
        private readonly IOptions<Settings> _options = Options.Create(new Settings { RunIndefinitely = false });

        public StartCommandTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
            _projectManager = new ProjectManager(_console, _cleanupServiceMock.Object, new ProjectFilter());
        }

        StartCommand CreateCommand()
        {
            return new StartCommand(_runnerServiceMock.Object, _projectManager, _configServiceMock.Object, _console, _options);
        }

        [Fact]
        public async Task Will_start_all_projects_by_default()
        {
            _configServiceMock.Setup(m => m.GetConfigurationAsync(It.IsAny<CommandOptions>()))
                .ReturnsAsync(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _runnerServiceMock.Setup(m => m.StartAsync(It.IsAny<IList<Project>>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .Callback((IList<Project> projects, CommandOptions options, CancellationToken token) =>
                {
                    foreach (var p in projects)
                    {
                        p.Processes.Add(new AppProcess(new Process(), AppTask.Start, AppStatus.Success));
                    }
                })
                .Returns((IList<Project> projects, CommandOptions options, CancellationToken token) => Task.FromResult(projects))
                .Verifiable();

            StartCommand command = CreateCommand();

            int statusCode = await command.OnExecute();

            Assert.Equal(0, statusCode);

            _configServiceMock.Verify();
            _runnerServiceMock.Verify();
        }
    }
}
