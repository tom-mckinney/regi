using Moq;
using Regi.Abstractions;
using Regi.CommandLine.Commands;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Commands
{
    public class TestCommandTests
    {
        private readonly TestConsole _console;
        private readonly Mock<IRunnerService> _runnerServiceMock = new Mock<IRunnerService>();
        private readonly IProjectManager _projectManager;
        private readonly Mock<IConfigurationService> _configServiceMock = new Mock<IConfigurationService>();
        private readonly ISummaryService _summaryService;

        public TestCommandTests(ITestOutputHelper testOutput)
        {
            _console = new TestConsole(testOutput);
            _projectManager = new ProjectManager(_console, new Mock<ICleanupService>().Object, new ProjectFilter());
            _summaryService = new SummaryService(_projectManager, new TestFileSystem(), _console);
        }

        TestCommand CreateCommand()
        {
            return new TestCommand(_runnerServiceMock.Object, _summaryService, _projectManager, _configServiceMock.Object, _console)
            {
                Name = null
            };
        }

        [Fact]
        public async Task Will_run_all_test_if_no_name_or_type_is_specified()
        {
            _configServiceMock.Setup(m => m.GetConfigurationAsync(It.IsAny<CommandOptions>()))
                .ReturnsAsync(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _runnerServiceMock.Setup(m => m.TestAsync(It.IsAny<IList<IProject>>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .Callback((IList<IProject> projects, CommandOptions options, CancellationToken token) =>
                {
                    foreach (var p in projects)
                    {
                        p.Processes.Add(new AppProcess(new Process(), AppTask.Test, AppStatus.Success));
                    }
                })
                .Returns((IList<IProject> projects, CommandOptions options, CancellationToken token) => Task.FromResult(projects))
                .Verifiable();

            TestCommand command = CreateCommand();

            int testProjectCount = await command.OnExecute();

            Assert.Equal(0, testProjectCount);

            _configServiceMock.Verify();
            _runnerServiceMock.Verify();
        }

        [Fact]
        public async Task Returns_fail_count_as_exit_code()
        {
            _configServiceMock.Setup(m => m.GetConfigurationAsync(It.IsAny<CommandOptions>()))
                .ReturnsAsync(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _runnerServiceMock.Setup(m => m.TestAsync(It.IsAny<IList<IProject>>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .Callback((IList<Project> projects, CommandOptions options, CancellationToken token) =>
                {
                    projects[0].Processes.Add(new AppProcess(new Process(), AppTask.Test, AppStatus.Failure));
                })
                .Returns((IList<IProject> projects, CommandOptions options, CancellationToken token) => Task.FromResult(projects))
                .Verifiable();

            TestCommand command = CreateCommand();

            int testProjectCount = await command.OnExecute();

            Assert.Equal(1, testProjectCount);

            _configServiceMock.Verify();
            _runnerServiceMock.Verify();
        }

        [Theory]
        [InlineData(ProjectRole.Test)]
        public async Task Will_only_run_tests_with_matching_role_if_specified(ProjectRole type)
        {
            _configServiceMock.Setup(m => m.GetConfigurationAsync(It.IsAny<CommandOptions>()))
                .ReturnsAsync(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _runnerServiceMock.Setup(m => m.TestAsync(
                It.Is<IList<IProject>>(projects => projects.All(p => p.Roles.Contains(type))),
                It.Is<CommandOptions>(o => o.Roles.Contains(type)),
                It.IsAny<CancellationToken>()))
                .Callback((IList<Project> projects, CommandOptions options, CancellationToken token) =>
                {
                    foreach (var p in projects)
                    {
                        p.Processes.Add(new AppProcess(new Process(), AppTask.Test, AppStatus.Success));
                    }
                })
                .Returns((IList<IProject> projects, CommandOptions options, CancellationToken token) => Task.FromResult(projects))
                .Verifiable();

            TestCommand command = CreateCommand();

            command.Roles = new ProjectRole[] { type };

            int testProjectCount = await command.OnExecute();

            Assert.Equal(0, testProjectCount);

            _configServiceMock.Verify();
            _runnerServiceMock.Verify();
        }
    }
}
