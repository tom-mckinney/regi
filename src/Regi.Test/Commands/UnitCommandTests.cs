using Moq;
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
    public class UnitCommandTests
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly TestConsole _console;
        private readonly Mock<IRunnerService> _runnerServiceMock;

        public UnitCommandTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            _console = new TestConsole(testOutput);

            _runnerServiceMock = new Mock<IRunnerService>();
        }

        [Fact]
        public void Will_run_all_test_if_no_name_is_specified()
        {
            _runnerServiceMock.Setup(m => m.TestAsync(It.IsAny<string>()))
                .Returns(new List<DotnetProcess>
                {
                    new DotnetProcess(new Process(), DotnetTask.Test, DotnetStatus.Success),
                    new DotnetProcess(new Process(), DotnetTask.Test, DotnetStatus.Success)
                })
                .Verifiable();

            UnitCommand command = new UnitCommand(_runnerServiceMock.Object)
            {
                Name = null
            };

            int testProjectCount = command.OnExecute();

            Assert.Equal(2, testProjectCount);

            _runnerServiceMock.VerifyAll();
        }
    }
}
