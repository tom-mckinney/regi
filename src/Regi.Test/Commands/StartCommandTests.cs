using Microsoft.Extensions.Options;
using Moq;
using Regi.Commands;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Commands
{
    public class StartCommandTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly Mock<IRunnerService> _runnerServiceMock = new Mock<IRunnerService>();
        private readonly IOptions<Settings> _options = Options.Create(new Settings { RunIndefinitely = false });

        public StartCommandTests(ITestOutputHelper output)
        {
            _output = output;
            _console = new TestConsole(output);
        }

        StartCommand CreateCommand()
        {
            return new StartCommand(_runnerServiceMock.Object, _console, _options);
        }

        [Fact]
        public void Will_start_all_projects_by_default()
        {
            _runnerServiceMock.Setup(m => m.Start(It.IsAny<RegiOptions>()))
                .Returns(new List<Project>
                {
                    new Project
                    {
                        Processes = new ConcurrentBag<AppProcess> { new AppProcess(new Process(), AppTask.Start, AppStatus.Success) }
                    },
                    new Project
                    {
                        Processes = new ConcurrentBag<AppProcess> { new AppProcess(new Process(), AppTask.Start, AppStatus.Success) }
                    }
                })
                .Verifiable();

            StartCommand command = CreateCommand();

            int projectCount = command.OnExecute();

            Assert.Equal(2, projectCount);
        }
    }
}
