using Moq;
using Regi.Frameworks;
using Regi.Models;
using Regi.Services;
using Regi.Test.Extensions;
using Regi.Test.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Frameworks
{
    [Collection(TestCollections.NoParallel)]
    public class DotnetTests : TestBase
    {
        private readonly TestConsole _console;
        private readonly IDotnet _service;
        private readonly Mock<IRuntimeInfo> _runtimeInfoMock = new Mock<IRuntimeInfo>();

        private readonly Project _successfulTests;
        private readonly Project _failedTests;
        private readonly Project _application;
        private readonly Project _applicationError;
        private readonly Project _applicationLong;

        public DotnetTests(ITestOutputHelper testOutput)
        {
            _console = new TestConsole(testOutput);
            _service = new Dotnet(_fileSystem, _console, new PlatformService(_runtimeInfoMock.Object, _fileSystem, _console));

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
        public async Task TestProject_on_success_returns_status()
        {
            AppProcess unitTest = await _service.Test(_successfulTests, _successfulTests.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Assert.Equal(AppTask.Test, unitTest.Task);
            Assert.Equal(AppStatus.Success, unitTest.Status);
        }

        [Fact]
        public async Task TestProject_verbose_on_success_prints_all_output()
        {
            AppProcess unitTest = await _service.Test(_successfulTests, _successfulTests.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Assert.Equal(AppTask.Test, unitTest.Task);
            Assert.Equal(AppStatus.Success, unitTest.Status);
            Assert.NotEmpty(_console.LogOutput);
        }

        [Fact]
        public async Task TestProject_on_failure_prints_only_exception_info()
        {
            AppProcess unitTest = await _service.Test(_failedTests, _failedTests.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Assert.Equal(AppTask.Test, unitTest.Task);
            Assert.Equal(AppStatus.Failure, unitTest.Status);
            Assert.Contains("Test Run Failed.", _console.LogOutput, StringComparison.InvariantCulture);
        }

        [Fact]
        public async Task TestProject_verbose_on_failure_prints_all_output()
        {
            AppProcess unitTest = await _service.Test(_failedTests, _failedTests.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Assert.Equal(AppTask.Test, unitTest.Task);
            Assert.Equal(AppStatus.Failure, unitTest.Status);
            Assert.Contains("Total tests:", _console.LogOutput, StringComparison.InvariantCulture);
        }

        [Fact]
        public async Task RunProject_changes_status_from_running_to_success_on_exit()
        {
            var options = TestOptions.Create();

            AppProcess process = await _service.Start(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], options, CancellationToken.None);

            Assert.Equal(AppStatus.Running, process.Status);

            await process.WaitForExitAsync(CancellationToken.None);

            Assert.Equal(AppStatus.Success, process.Status);

            CleanupApp(process);
        }

        [Fact]
        public async Task RunProject_returns_failure_status_on_thrown_exception()
        {
            AppProcess process = await _service.Start(_applicationError, _applicationError.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            await process.WaitForExitAsync(CancellationToken.None);

            Assert.Equal(AppStatus.Failure, process.Status);

            CleanupApp(process);
        }

        [Fact]
        public async Task RunProject_without_verbose_starts_and_prints_nothing()
        {
            CommandOptions optionsWithoutVerbose = new CommandOptions { Verbose = false, KillProcessesOnExit = false };

            AppProcess process = await _service.Start(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], optionsWithoutVerbose, CancellationToken.None);

            process.WaitForExit();

            await Task.Delay(100); // flakes out for some reason

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Null(_console.LogOutput);

            CleanupApp(process);
        }

        [Fact]
        public async Task RunProject_verbose_starts_and_prints_all_output()
        {
            AppProcess process = await _service.Start(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            process.WaitForExit();

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Contains("Hello World!", _console.LogOutput, StringComparison.InvariantCulture);

            CleanupApp(process);
        }

        [Fact]
        public async Task RunProject_long_starts_and_prints_nothing()
        {
            AppProcess process = await _service.Start(_applicationLong, _applicationLong.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Thread.Sleep(1000);

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Running, process.Status);
            Assert.Null(process.Port);

            CleanupApp(process);
        }

        [Fact]
        public async Task RunProject_will_start_custom_port_if_specified()
        {
            _applicationLong.Port = 8080;

            AppProcess process = await _service.Start(_applicationLong, _applicationLong.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Thread.Sleep(1000);

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Running, process.Status);
            Assert.Equal(8080, process.Port);

            CleanupApp(process);
        }

        [Fact]
        public async Task RunProject_will_add_all_variables_passed_to_process()
        {
            _applicationLong.Port = 8080;

            EnvironmentVariableDictionary varList = new EnvironmentVariableDictionary
            {
                { "foo", "bar" }
            };

            AppProcess process = await _service.Start(_applicationLong, _applicationLong.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(varList), CancellationToken.None);

            Thread.Sleep(500);

            Assert.Equal(AppTask.Start, process.Task);
            Assert.Equal(AppStatus.Running, process.Status);
            Assert.Equal(8080, process.Port);
            Assert.True(process.Process.StartInfo.EnvironmentVariables.ContainsKey("foo"), "Environment variable \"foo\" has not been set.");
            Assert.Equal("bar", process.Process.StartInfo.EnvironmentVariables["foo"]);

            CleanupApp(process);
        }

        [Fact]
        public async Task InstallProject_returns_process()
        {
            _application.Source = "http://artifactory.org/nuget";

            AppProcess process = await _service.Install(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Assert.Equal(AppTask.Install, process.Task);
            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Null(process.Port);
        }

        [Fact]
        public async Task InstallProject_sets_source_if_specified()
        {
            string source = "http://artifactory.org/nuget";
            _application.Source = source;

            AppProcess process = await _service.Install(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Assert.Contains($"--source {source}", process.Process.StartInfo.Arguments, StringComparison.InvariantCulture);
        }

        [Theory]
        [InlineData(true, "taskkill", "/F /IM dotnet.exe")]
        [InlineData(false, "killall", "dotnet")]
        public async Task KillProcesses_kills_all_dotnet_processes(bool isWindows, string expectedFileName, string expectedArguments)
        {
            _runtimeInfoMock.Setup(m => m.IsWindows).Returns(isWindows).Verifiable();

            AppProcess process = await _service.Kill(TestOptions.Create(), CancellationToken.None);

            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Equal(AppTask.Kill, process.Task);

            var startInfo = process.Process.StartInfo;
            Assert.Equal(expectedFileName, startInfo.FileName);
            Assert.Equal(expectedArguments, startInfo.Arguments);

            _runtimeInfoMock.Verify();
        }

        [Fact]
        public async Task ShutdownBuildServer_shuts_down_all_build_servers()
        {
            _fileSystem.WorkingDirectory = PathHelper.RegiTestRootPath;

            var process =  await _service.ShutdownBuildServer(TestOptions.Create(), CancellationToken.None);

            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Equal(AppTask.Cleanup, process.Task);

            var startInfo = process.Process.StartInfo;
            Assert.Contains("dotnet", startInfo.FileName, StringComparison.InvariantCulture);
            Assert.Equal(FrameworkCommands.DotnetCore.ShutdownBuildServer, startInfo.Arguments);
        }

        [Fact]
        public void BuildCommand_does_not_add_command_options_if_restoring()
        {
            var project = SampleProjects.Backend;
            project.Arguments.AddOptions("*", "--dont-do-this-on-restore");
            project.Arguments.AddOptions("restore", "--foo bar");

            var command = ((Dotnet)_service).BuildCommandArguments(FrameworkCommands.DotnetCore.Restore, project, TestOptions.Create());

            Assert.Equal("restore --foo bar", command);
        }
    }
}
