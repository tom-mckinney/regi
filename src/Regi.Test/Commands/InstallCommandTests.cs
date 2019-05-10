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
        private readonly Mock<IRunnerService> _runnerServiceMock;

        public InstallCommandTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            _console = new TestConsole(testOutput);

            _runnerServiceMock = new Mock<IRunnerService>();
        }

        [Fact]
        public void Will_install_dependencies_for_all_projects_by_default()
        {
            _runnerServiceMock.Setup(m => m.Install(It.IsAny<RegiOptions>()))
                .Returns(new List<Project>
                {
                    new Project
                    {
                        Process = new AppProcess(new Process(), AppTask.Install, AppStatus.Success)
                    },
                    new Project
                    {
                        Process = new AppProcess(new Process(), AppTask.Install, AppStatus.Success)
                    }
                })
                .Verifiable();

            InstallCommand command = new InstallCommand(_runnerServiceMock.Object, _console)
            {
                Name = null
            };

            int testProjectCount = command.OnExecute();

            Assert.Equal(2, testProjectCount);

            _runnerServiceMock.VerifyAll();
        }
    }
}
