using Moq;
using Regi.Commands;
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
    public class InstallCommandTests
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly TestConsole _console;
        private readonly Mock<IRunnerService> _runnerServiceMock = new Mock<IRunnerService>();
        private readonly IProjectManager _projectManager;
        private readonly Mock<IConfigurationService> _configServiceMock = new Mock<IConfigurationService>();

        public InstallCommandTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            _console = new TestConsole(testOutput);
            _projectManager = new ProjectManager(_console, new Mock<ICleanupService>().Object);
        }

        InstallCommand CreateCommand()
        {
            return new InstallCommand(_runnerServiceMock.Object, _projectManager, _configServiceMock.Object, _console)
            {
                Name = null
            };
        }

        [Fact]
        public async Task Will_install_dependencies_for_all_projects_by_default()
        {
            _configServiceMock.Setup(m => m.GetConfigurationAsync(It.IsAny<RegiOptions>()))
                .ReturnsAsync(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _runnerServiceMock.Setup(m => m.InstallAsync(It.IsAny<IList<Project>>(), It.IsAny<RegiOptions>(), It.IsAny<CancellationToken>()))
                .Callback((IList<Project> projects, RegiOptions options, CancellationToken token) =>
                {
                    foreach (var p in projects)
                    {
                        p.Processes.Add(new AppProcess(new Process(), AppTask.Install, AppStatus.Success));
                    }
                })
                .Returns((IList<Project> projects, RegiOptions options, CancellationToken token) => Task.FromResult(projects))
                .Verifiable();

            InstallCommand command = CreateCommand();

            int statusCode = await command.OnExecute();

            Assert.Equal(0, statusCode);

            _configServiceMock.Verify();
            _runnerServiceMock.Verify();
        }

        [Fact]
        public async Task Returns_fail_count_as_exit_code()
        {
            _configServiceMock.Setup(m => m.GetConfigurationAsync(It.IsAny<RegiOptions>()))
                .ReturnsAsync(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _runnerServiceMock.Setup(m => m.InstallAsync(It.IsAny<IList<Project>>(), It.IsAny<RegiOptions>(), It.IsAny<CancellationToken>()))
                .Callback((IList<Project> projects, RegiOptions options, CancellationToken token) =>
                {
                    projects[0].Processes.Add(new AppProcess(new Process(), AppTask.Install, AppStatus.Failure));
                })
                .Returns((IList<Project> projects, RegiOptions options, CancellationToken token) => Task.FromResult(projects))
                .Verifiable();

            InstallCommand command = CreateCommand();

            int testProjectCount = await command.OnExecute();

            Assert.Equal(1, testProjectCount);

            _configServiceMock.Verify();
            _runnerServiceMock.Verify();
        }
    }
}
