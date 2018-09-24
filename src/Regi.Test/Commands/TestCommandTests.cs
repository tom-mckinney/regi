using Moq;
using Regi.Commands;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Commands
{
    public class TestCommandTests
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly TestConsole _console;
        private readonly Mock<IRunnerService> _runnerServiceMock;

        public TestCommandTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            _console = new TestConsole(testOutput);

            _runnerServiceMock = new Mock<IRunnerService>();
        }

        [Fact]
        public void Will_run_all_test_if_no_name_or_type_is_specified()
        {
            _runnerServiceMock.Setup(m => m.Test(It.IsAny<string>(), null))
                .Returns(new List<AppProcess>
                {
                    new AppProcess(new Process(), AppTask.Test, AppStatus.Success),
                    new AppProcess(new Process(), AppTask.Test, AppStatus.Success)
                })
                .Verifiable();

            TestCommand command = new TestCommand(_runnerServiceMock.Object, _console)
            {
                Name = null
            };

            int testProjectCount = command.OnExecute();

            Assert.Equal(2, testProjectCount);

            _runnerServiceMock.VerifyAll();
        }

        [Theory]
        [InlineData(ProjectType.Unit)]
        [InlineData(ProjectType.Integration)]
        [InlineData(null)]
        public void Will_only_run_tests_with_matching_type_if_specified(ProjectType? type)
        {
            _runnerServiceMock.Setup(m => m.Test(It.IsAny<string>(), type))
                .Returns(new List<AppProcess>
                {
                    new AppProcess(new Process(), AppTask.Test, AppStatus.Success)
                })
                .Verifiable();

            TestCommand command = new TestCommand(_runnerServiceMock.Object, _console)
            {
                Name = null,
                Type = type
            };

            int testProjectCount = command.OnExecute();

            Assert.Equal(1, testProjectCount);

            _runnerServiceMock.VerifyAll();
        }
    }
}
