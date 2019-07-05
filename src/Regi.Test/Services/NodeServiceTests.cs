using McMaster.Extensions.CommandLineUtils;
using Moq;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    [Collection("Integration")]
    public class NodeServiceTests
    {
        private readonly IConsole _console;
        private readonly Mock<IRuntimeInfo> _runtimeInfoMock = new Mock<IRuntimeInfo>();
        private readonly INodeService _service;

        private readonly Project _application = SampleProjects.SimpleNodeApp;

        public NodeServiceTests(ITestOutputHelper testOutput)
        {
            _console = new TestConsole(testOutput);
            _service = new NodeService(_console, new PlatformService(_console, _runtimeInfoMock.Object));
        }

        [Fact]
        public void RunProject_starts_and_returns_process()
        {
            AppProcess app = _service.StartProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create());

            Thread.Sleep(1000);

            Assert.Equal(AppTask.Start, app.Task);
            Assert.Equal(AppStatus.Running, app.Status);
        }

        [Fact]
        public void RunProject_will_start_custom_port_if_specified()
        {
            int expectedPort = new Random().Next(1000, 9999);

            _application.Port = expectedPort;

            AppProcess app = _service.StartProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create());

            Thread.Sleep(1000);

            Assert.Equal(AppTask.Start, app.Task);
            Assert.Equal(AppStatus.Running, app.Status);
            Assert.Equal(expectedPort, app.Port);
        }

        [Fact]
        public void RunProject_will_add_all_variables_passed_to_process()
        {
            _application.Port = 8080;

            VariableList varList = new VariableList
            {
                { "foo", "bar" }
            };

            AppProcess appProcess = _service.StartProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create(varList));

            Thread.Sleep(500);

            Assert.Equal(AppTask.Start, appProcess.Task);
            Assert.Equal(AppStatus.Running, appProcess.Status);
            Assert.Equal(8080, appProcess.Port);
            Assert.True(appProcess.Process.StartInfo.EnvironmentVariables.ContainsKey("foo"), "Environment variable \"foo\" has not been set.");
            Assert.Equal("bar", appProcess.Process.StartInfo.EnvironmentVariables["foo"]);
        }

        [Theory]
        [InlineData(null, AppStatus.Failure)]
        [InlineData("passing", AppStatus.Success)]
        [InlineData("failing", AppStatus.Failure)]
        public void TestProject_will_return_test_for_path_pattern_and_expected_status(string pathPattern, AppStatus expectedStatus)
        {
            AppProcess test = _service.TestProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create(null, pathPattern));

            Assert.Equal(AppTask.Test, test.Task);
            Assert.Equal(expectedStatus, test.Status);
        }

        [Fact]
        public void TestProject_will_add_all_variables_passed_to_process()
        {
            _application.Port = 8080;

            VariableList varList = new VariableList
            {
                { "foo", "bar" }
            };

            AppProcess testProcess = _service.TestProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create(varList));

            Assert.Equal(AppTask.Test, testProcess.Task);
            Assert.Equal(AppStatus.Failure, testProcess.Status);
            Assert.Equal(8080, testProcess.Port);
            Assert.True(testProcess.Process.StartInfo.EnvironmentVariables.ContainsKey("foo"), "Environment variable \"foo\" has not been set.");
            Assert.Equal("bar", testProcess.Process.StartInfo.EnvironmentVariables["foo"]);
        }

        [Fact]
        public void InstallProject_returns_process()
        {
            AppProcess process = _service.InstallProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create());

            Assert.Equal(AppTask.Install, process.Task);
            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Equal(_application.Port, process.Port);
        }

        [Fact]
        public void InstallProjects_sets_registry_url_if_specified()
        {
            string source = "https://artifactory.org/npm";
            _application.Source = source;

            AppProcess process = _service.InstallProject(_application, _application.AppDirectoryPaths[0], TestOptions.Create());

            Assert.Contains($"--registry {source}", process.Process.StartInfo.Arguments);
        }

        [Theory]
        [InlineData(true, "taskkill", "/F /IM node.exe")]
        [InlineData(false, "killall", "node")]
        public void KillProcesses_kills_all_node_processes(bool isWindows, string expectedFileName, string expectedArguments)
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
    }
}
