﻿using Moq;
using Regi.Commands;
using Regi.Models;
using Regi.Services;
using Regi.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Commands
{
    public class StartCommandTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly Mock<IRunnerService> _runnerServiceMock;

        public StartCommandTests(ITestOutputHelper output)
        {
            _output = output;
            _console = new TestConsole(output);

            _runnerServiceMock = new Mock<IRunnerService>();
        }

        [Fact]
        public void Will_start_all_projects_by_default()
        {
            _runnerServiceMock.Setup(m => m.RunAsync(It.IsAny<string>()))
                .Returns(new List<DotnetProcess>
                {
                    new DotnetProcess(new Process(), DotnetTask.Run, DotnetStatus.Success),
                    new DotnetProcess(new Process(), DotnetTask.Run, DotnetStatus.Success)
                })
                .Verifiable();

            StartCommand command = new StartCommand(_runnerServiceMock.Object, _console);

            int projectCount = command.OnExecute();

            Assert.Equal(2, projectCount);
        }
    }
}