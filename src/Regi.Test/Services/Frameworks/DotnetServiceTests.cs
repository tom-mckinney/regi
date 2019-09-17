using McMaster.Extensions.CommandLineUtils;
using Moq;
using Regi.Models;
using Regi.Services;
using Regi.Services.Frameworks;
using Regi.Test.Helpers;
using Regi.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services.Frameworks
{
    [Collection(TestCollections.NoParallel)]
    public class DotnetServiceTests : TestBase
    {
        private readonly TestConsole _console;
        private readonly IDotnetService _service;
        private readonly Mock<IRuntimeInfo> _runtimeInfoMock = new Mock<IRuntimeInfo>();
        private readonly object _lock = new object();

        private readonly Project _successfulTests;
        private readonly Project _failedTests;
        private readonly Project _application;
        private readonly Project _applicationError;
        private readonly Project _applicationLong;

        public DotnetServiceTests(ITestOutputHelper testOutput)
        {
            DirectoryUtility.ResetTargetDirectory();

            _console = new TestConsole(testOutput);
            _service = new DotnetService(_console, new PlatformService(_console, _runtimeInfoMock.Object));

            _successfulTests = new Project("SampleSuccessfulTests", new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleSuccessfulTests.csproj", SearchOption.AllDirectories)
                .First()
                .FullName);
            _failedTests = new Project("SampleFailedTests", new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleFailedTests.csproj", SearchOption.AllDirectories)
                .First()
                .FullName);
            _application = new Project("SampleApp", new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleApp.csproj", SearchOption.AllDirectories)
                .First()
                .FullName);
            _applicationError = new Project("SampleAppError", new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleAppError.csproj", SearchOption.AllDirectories)
                .First()
                .FullName);
            _applicationLong = new Project("SampleAppLong", new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleAppLong.csproj", SearchOption.AllDirectories)
                .First()
                .FullName);
        }

        [Fact]
        public void TestProject_on_success_returns_status()
        {
            AppProcess unitTest = _service.TestProject(_successfulTests, _successfulTests.AppDirectoryPaths[0], TestOptions.Create());

            Assert.Equal(AppTask.Test, unitTest.Task);
            Assert.Equal(AppStatus.Success, unitTest.Status);
        }

        [Fact]
        public void TestProject_verbose_on_success_prints_all_output()
        {
            AppProcess unitTest = _service.TestProject(_successfulTests, _successfulTests.AppDirectoryPaths[0], TestOptions.Create());

            Assert.Equal(AppTask.Test, unitTest.Task);
            Assert.Equal(AppStatus.Success, unitTest.Status);
            Assert.NotEmpty(_console.LogOutput);
        }

        [Fact]
        public void TestProject_on_failure_prints_only_exception_info()
        {
            AppProcess unitTest = _service.TestProject(_failedTests, _failedTests.AppDirectoryPaths[0], TestOptions.Create());

            Assert.Equal(AppTask.Test, unitTest.Task);
            Assert.Equal(AppStatus.Failure, unitTest.Status);
            Assert.Contains("Test Run Failed.", _console.LogOutput, StringComparison.InvariantCulture);
        }

        [Fact]
        public void TestProject_verbose_on_failure_prints_all_output()
        {
            AppProcess unitTest = _service.TestProject(_failedTests, _failedTests.AppDirectoryPaths[0], TestOptions.Create());

            Assert.Equal(AppTask.Test, unitTest.Task);
            Assert.Equal(AppStatus.Failure, unitTest.Status);
            Assert.Contains("Total tests:", _console.LogOutput, StringComparison.InvariantCulture);
        }

        [Fact]
        public void RunProject_changes_status_from_running_to_success_on_exit()
        {
            lock (_lock)
            {
                var options = TestOptions.Create();

                options.VariableList = new EnvironmentVariableDictionary
                {
                    { "DOTNET_ROOT", System.Environment.GetEnvironmentVariable("DOTNET_ROOT") }
                };

                AppProcess process = _service.StartProject(_application, _application.AppDirectoryPaths[0], options);

                Assert.Equal(AppStatus.Running, process.Status);

                process.WaitForExit();

                Assert.Equal(AppStatus.Success, process.Status);

                CleanupApp(process);
            }
        }

        [Fact]
        public void RunProject_returns_failure_status_on_thrown_exception()
        {
            lock (_lock)
            {
                AppProcess process = _service.StartProject(_applicationError, _applicationError.AppDirectoryPaths[0], TestOptions.Create());

                process.WaitForExit();

                Assert.Equal(AppStatus.Failure, process.Status);

                CleanupApp(process);
            }
        }

        [Fact]
        public void RunProject_without_verbose_starts_and_prints_nothing()
        {
            RegiOptions optionsWithoutVerbose = new RegiOptions { Verbose = false, KillProcessesOnExit = false };

            AppProcess process = _service.StartProject(_application, _application.AppDirectoryPaths[0], optionsWithoutVerbose);

            process.WaitForExit();

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Null(_console.LogOutput);

            CleanupApp(process);
        }

        [Fact]
        public void RunProject_verbose_starts_and_prints_all_output()
        {
            AppProcess process = _service.StartProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create());

            process.WaitForExit();

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Contains("Hello World!", _console.LogOutput, StringComparison.InvariantCulture);

            CleanupApp(process);
        }

        [Fact]
        public void RunProject_long_starts_and_prints_nothing()
        {
            AppProcess process = _service.StartProject(_applicationLong, _applicationLong.AppDirectoryPaths[0], TestOptions.Create());

            Thread.Sleep(1000);

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Running, process.Status);
            Assert.Null(process.Port);

            CleanupApp(process);
        }

        [Fact]
        public void RunProject_will_start_custom_port_if_specified()
        {
            _applicationLong.Port = 8080;

            AppProcess process = _service.StartProject(_applicationLong, _applicationLong.AppDirectoryPaths[0], TestOptions.Create());

            Thread.Sleep(1000);

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Running, process.Status);
            Assert.Equal(8080, process.Port);

            CleanupApp(process);
        }

        [Fact]
        public void RunProject_will_add_all_variables_passed_to_process()
        {
            _applicationLong.Port = 8080;

            EnvironmentVariableDictionary varList = new EnvironmentVariableDictionary
            {
                { "foo", "bar" }
            };

            AppProcess process = _service.StartProject(_applicationLong, _applicationLong.AppDirectoryPaths[0], TestOptions.Create(varList));

            Thread.Sleep(500);

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Running, process.Status);
            Assert.Equal(8080, process.Port);
            Assert.True(process.Process.StartInfo.EnvironmentVariables.ContainsKey("foo"), "Environment variable \"foo\" has not been set.");
            Assert.Equal("bar", process.Process.StartInfo.EnvironmentVariables["foo"]);

            CleanupApp(process);
        }

        [Fact]
        public void InstallProject_returns_process()
        {
            _application.Source = "http://artifactory.org/nuget";

            AppProcess process = _service.InstallProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create());

            Assert.Equal(AppTask.Install, process.Task);
            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Null(process.Port);
        }

        [Fact]
        public void InstallProject_sets_source_if_specified()
        {
            string source = "http://artifactory.org/nuget";
            _application.Source = source;

            AppProcess process = _service.InstallProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create());

            Assert.Contains($"--source {source}", process.Process.StartInfo.Arguments, StringComparison.InvariantCulture);
        }

        [Theory]
        [InlineData(true, "taskkill", "/F /IM dotnet.exe")]
        [InlineData(false, "killall", "dotnet")]
        public void KillProcesses_kills_all_dotnet_processes(bool isWindows, string expectedFileName, string expectedArguments)
        {
            _runtimeInfoMock.Setup(m => m.IsWindows).Returns(isWindows).Verifiable();

            AppProcess process = _service.KillProcesses(TestOptions.Create());

            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Equal(AppTask.Kill, process.Task);

            var startInfo = process.Process.StartInfo;
            Assert.Equal(expectedFileName, startInfo.FileName);
            Assert.Equal(expectedArguments, startInfo.Arguments);

            _runtimeInfoMock.Verify();
        }

        [Fact]
        public void ShutdownBuildServer_shuts_down_all_build_servers()
        {
            DirectoryUtility.SetTargetDirectory(PathHelper.ProjectRootPath);

            var process = _service.ShutdownBuildServer(TestOptions.Create());

            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Equal(AppTask.Cleanup, process.Task);

            var startInfo = process.Process.StartInfo;
            Assert.Contains("dotnet", startInfo.FileName, StringComparison.InvariantCulture);
            Assert.Equal(FrameworkCommands.Dotnet.ShutdownBuildServer, startInfo.Arguments);
        }

        [Fact]
        public void BuildCommand_does_not_add_command_options_if_restoring()
        {
            var project = SampleProjects.Backend;
            project.Options.AddOptions("*", "--dont-do-this-on-restore");
            project.Options.AddOptions("restore", "--foo bar");

            var command = ((DotnetService)_service).BuildCommand(FrameworkCommands.Dotnet.Restore, project, TestOptions.Create());

            Assert.Equal("restore --foo bar", command);
        }
    }
}
