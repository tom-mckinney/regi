using Moq;
using Regi.Commands;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
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
        public void Will_install_dependencies_for_all_projects_by_default()
        {
            _configServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _runnerServiceMock.Setup(m => m.Install(It.IsAny<IList<Project>>(), It.IsAny<RegiOptions>()))
                .Callback<IList<Project>, RegiOptions>((projects, options) =>
                {
                    foreach (var p in projects)
                    {
                        p.Processes.Add(new AppProcess(new Process(), AppTask.Install, AppStatus.Success));
                    }
                })
                .Returns<IList<Project>, RegiOptions>((projects, options) => projects)
                .Verifiable();

            InstallCommand command = CreateCommand();

            int statusCode = command.OnExecute();

            Assert.Equal(0, statusCode);

            _configServiceMock.Verify();
            _runnerServiceMock.Verify();
        }

        [Fact]
        public void Returns_fail_count_as_exit_code()
        {
            _configServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _runnerServiceMock.Setup(m => m.Install(It.IsAny<IList<Project>>(), It.IsAny<RegiOptions>()))
                .Callback<IList<Project>, RegiOptions>((projects, options) =>
                {
                    projects[0].Processes.Add(new AppProcess(new Process(), AppTask.Install, AppStatus.Failure));
                })
                .Returns<IList<Project>, RegiOptions>((projects, options) => projects)
                .Verifiable();

            InstallCommand command = CreateCommand();

            int testProjectCount = command.OnExecute();

            Assert.Equal(1, testProjectCount);

            _configServiceMock.Verify();
            _runnerServiceMock.Verify();
        }
    }
}
