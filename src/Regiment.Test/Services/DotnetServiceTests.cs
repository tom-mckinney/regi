using McMaster.Extensions.CommandLineUtils;
using Moq;
using Regiment.Extensions;
using Regiment.Models;
using Regiment.Services;
using Regiment.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Regiment.Test.Services
{
    public class DotnetServiceTests
    {
        private readonly TestConsole _console;
        private readonly IDotnetService _service;

        private readonly FileInfo _successfulTests;
        private readonly FileInfo _failedTests;
        private readonly FileInfo _application;
        private readonly FileInfo _applicationError;
        private readonly FileInfo _applicationLong;

        public DotnetServiceTests(ITestOutputHelper testOutput)
        {
            _console = new TestConsole(testOutput);
            _service = new DotnetService(_console);

            _successfulTests = new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleSuccessfulTests.csproj", SearchOption.AllDirectories)
                .First();
            _failedTests = new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleFailedTests.csproj", SearchOption.AllDirectories)
                .First();
            _application = new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleApp.csproj", SearchOption.AllDirectories)
                .First();
            _applicationError = new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleAppError.csproj", SearchOption.AllDirectories)
                .First();
            _applicationLong = new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleAppLong.csproj", SearchOption.AllDirectories)
                .First();
        }

        [Fact]
        public void TestProject_on_success_prints_prints_nothing()
        {
            DotnetProcess unitTest = _service.TestProject(_successfulTests);

            Assert.Equal(DotnetTask.Test, unitTest.Task);
            Assert.Equal(DotnetStatus.Success, unitTest.Status);
            Assert.Null(_console.LogOutput);
        }

        [Fact]
        public void TestProject_verbose_on_success_prints_all_output()
        {
            DotnetProcess unitTest = _service.TestProject(_successfulTests, true);

            Assert.Equal(DotnetTask.Test, unitTest.Task);
            Assert.Equal(DotnetStatus.Success, unitTest.Status);
            Assert.NotEmpty(_console.LogOutput);
        }

        [Fact]
        public void TestProject_on_failure_prints_only_exception_info()
        {
            DotnetProcess unitTest = _service.TestProject(_failedTests);

            Assert.Equal(DotnetTask.Test, unitTest.Task);
            Assert.Equal(DotnetStatus.Failure, unitTest.Status);
            Assert.Contains("Test Run Failed.", _console.LogOutput);
        }

        [Fact]
        public void TestProject_verbose_on_failure_prints_all_output()
        {
            DotnetProcess unitTest = _service.TestProject(_failedTests, true);

            Assert.Equal(DotnetTask.Test, unitTest.Task);
            Assert.Equal(DotnetStatus.Failure, unitTest.Status);
            Assert.Contains("Total tests:", _console.LogOutput);
        }

        [Fact]
        public void RunProject_changes_status_from_running_to_success_on_exit()
        {
            DotnetProcess app = _service.RunProject(_application);

            Assert.Equal(DotnetStatus.Running, app.Status);

            app.Process.WaitForExit();

            Assert.Equal(DotnetStatus.Success, app.Status);
        }

        [Fact]
        public void RunProject_returns_failure_status_on_thrown_exception()
        {
            DotnetProcess app = _service.RunProject(_applicationError);

            app.Process.WaitForExit();

            Assert.Equal(DotnetStatus.Failure, app.Status);
        }

        [Fact]
        public void RunProject_starts_and_prints_nothing()
        {
            DotnetProcess app = _service.RunProject(_application);

            app.Process.WaitForExit();

            Assert.Equal(DotnetTask.Run, app.Task);
            Assert.Equal(DotnetStatus.Success, app.Status);
            Assert.Null(_console.LogOutput);
        }

        [Fact]
        public void RunProject_verbose_starts_and_prints_all_output()
        {
            DotnetProcess app = _service.RunProject(_application, true);

            app.Process.WaitForExit();

            Assert.Equal(DotnetTask.Run, app.Task);
            Assert.Equal(DotnetStatus.Success, app.Status);
            Assert.Contains("Hello World!", _console.LogOutput);
        }

        [Fact]
        public void RunProject_long_starts_and_prints_nothing()
        {
            using (DotnetProcess app = _service.RunProject(_applicationLong, true))
            {
                Thread.Sleep(1000);

                Assert.Equal(DotnetTask.Run, app.Task);
                Assert.Equal(DotnetStatus.Running, app.Status);
            }
        }
    }
}
