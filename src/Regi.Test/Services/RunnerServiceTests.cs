using Moq;
using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using Regi.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class RunnerServiceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly Mock<IConfigurationService> _configurationServiceMock = new Mock<IConfigurationService>(MockBehavior.Strict);
        private readonly Mock<IDotnetService> _dotnetServiceMock = new Mock<IDotnetService>();
        private readonly Mock<INodeService> _nodeServiceMock = new Mock<INodeService>();
        private readonly Mock<IFrameworkServiceProvider> _frameworkServiceProviderMock = new Mock<IFrameworkServiceProvider>(MockBehavior.Loose);
        private readonly TestParallelService _queueService;
        private readonly Mock<INetworkingService> _networkingServiceMock = new Mock<INetworkingService>();
        private readonly Mock<IFileService> _fileServiceMock = new Mock<IFileService>();
        private readonly IRunnerService _runnerService;

        public RunnerServiceTests(ITestOutputHelper output)
        {
            _frameworkServiceProviderMock.Setup(m => m.GetFrameworkService(ProjectFramework.Dotnet))
                .Returns(_dotnetServiceMock.Object);
            _frameworkServiceProviderMock.Setup(m => m.GetFrameworkService(ProjectFramework.Node))
                .Returns(_nodeServiceMock.Object);

            _output = output;
            _console = new TestConsole(output);
            _queueService = new TestParallelService(_console);
            _runnerService = new RunnerService(
                _configurationServiceMock.Object,
                _frameworkServiceProviderMock.Object,
                _queueService,
                _networkingServiceMock.Object,
                _fileServiceMock.Object,
                _console);
        }

        [Fact]
        public void Start_returns_a_project_for_every_app_in_startup_config()
        {
            var configuration = SampleProjects.ConfigurationGood;

            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(configuration)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns<Project, RegiOptions>((p, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns<Project, RegiOptions>((p, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var processes = _runnerService.Start(TestOptions.Create());

            Assert.Equal(configuration.Apps.Count, processes.Count);
            Assert.Single(processes, p => p.Port == 9080);
            Assert.Equal(processes.Count(p => p.Port.HasValue), _queueService.ActivePorts.Count);

            _configurationServiceMock.Verify();
            _dotnetServiceMock.Verify(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()), Times.Exactly(configuration.Apps.Count(a => a.Framework == ProjectFramework.Dotnet)));
            _nodeServiceMock.Verify(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()), Times.Exactly(configuration.Apps.Count(a => a.Framework == ProjectFramework.Node)));
        }

        [Fact]
        public void Start_sets_port_and_url_variables_for_every_app_and_waits_on_port()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns<Project, RegiOptions>((p, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns<Project, RegiOptions>((p, b) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var projects = _runnerService.Start(TestOptions.Create());

            foreach (var p in projects)
            {
                if (p.Port.HasValue)
                {
                    _dotnetServiceMock.Verify(m => m.StartProject(It.IsAny<Project>(),
                        It.Is<RegiOptions>(o => o.VariableList.ContainsKey($"{p.Name}_PORT".ToUnderscoreCase()) &&
                        o.VariableList.ContainsKey($"{p.Name}_URL".ToUnderscoreCase()))));

                    Assert.Contains(p.Port.Value, _queueService.ActivePorts);
                }
            }
        }

        [Fact]
        public void Start_adds_serial_projects_to_serial_queue_and_all_others_to_parallel_queue()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns<Project, RegiOptions>((p, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns<Project, RegiOptions>((p, b) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var projects = _runnerService.Start(TestOptions.Create());

            Assert.Equal(2, _queueService.ParallelActions.Count);
            Assert.Single(_queueService.SerialActions);
        }

        [Fact]
        public void Test_returns_a_project_for_every_test_in_startup_config()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns<Project, RegiOptions>((p, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var processes = _runnerService.Test(TestOptions.Create());

            Assert.Equal(SampleProjects.ConfigurationGood.Tests.Count, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        [Fact]
        public void Test_also_starts_every_requirement_for_each_test()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _nodeServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Start, AppStatus.Success))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Start, AppStatus.Success))
                .Verifiable();

            var processes = _runnerService.Test(TestOptions.Create());

            Assert.Equal(SampleProjects.ConfigurationDefault.Tests.Count, processes.Count);
            foreach (var project in processes)
            {
                Assert.Equal(AppTask.Test, project.Process.Task);
                if (project.Requires?.Count > 0)
                {
                    Assert.Equal(project.Requires.Count, project.RequiredProjects.Count);
                    foreach (var requirement in project.Requires)
                    {
                        var requiredApp = Assert.Single(project.RequiredProjects, p => p.Name == requirement);
                        Assert.Equal(AppTask.Start, requiredApp.Process.Task);
                        Assert.Contains(requiredApp.Port.Value, _queueService.ActivePorts);
                    }
                }
            }

            _dotnetServiceMock.VerifyAll();
            _nodeServiceMock.VerifyAll();
        }

        [Theory]
        [InlineData(ProjectType.Unit, 1)]
        [InlineData(ProjectType.Integration, 1)]
        public void Test_will_only_run_tests_on_type_specified(ProjectType type, int expectedCount)
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();

            var processes = _runnerService.Test(new RegiOptions { Type = type });

            Assert.Equal(expectedCount, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        [Fact]
        public void Test_adds_serial_projects_to_serial_queue_and_all_others_to_parallel_queue()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns<Project, RegiOptions>((p, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var processes = _runnerService.Test(TestOptions.Create());

            Assert.Single(_queueService.ParallelActions);
            Assert.Single(_queueService.SerialActions);
        }

        [Fact]
        public void Install_returns_a_process_for_every_app_and_test_project()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var processes = _runnerService.Install(TestOptions.Create());

            int appsCount = SampleProjects.ConfigurationGood.Apps.Count;
            int testsCount = SampleProjects.ConfigurationGood.Tests.Count;
            Assert.Equal(appsCount + testsCount, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        [Fact]
        public void Install_sets_package_repo_to_value_configured_startup_file()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var processes = _runnerService.Install(TestOptions.Create());

            var dotnetApp = Assert.Single(processes, p => p.Framework == ProjectFramework.Dotnet && p.Type == ProjectType.Web && p.Port.HasValue);
            var nodeApp = Assert.Single(processes, p => p.Framework == ProjectFramework.Node && p.Type == ProjectType.Web && p.Port.HasValue);

            Assert.Equal("http://nuget.org/api", dotnetApp.Source);
            Assert.Equal("http://npmjs.org", nodeApp.Source);
        }

        [Fact]
        public void Initialize_returns_a_single_process_and_creates_a_config_file()
        {
            _fileServiceMock.Setup(m => m.CreateConfigFile())
                .Returns(new FileInfo("regi.json"))
                .Verifiable();

            DirectoryUtility.SetTargetDirectory(PathHelper.SampleDirectoryPath("ConfigurationNew"));

            _runnerService.Initialize(TestOptions.Create());

            _fileServiceMock.VerifyAll();
        }

        [Fact]
        public void Kill_calls_service_to_kill_for_each_ProjectFramework()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
               .Returns(SampleProjects.ConfigurationGood)
               .Verifiable();
            _nodeServiceMock.Setup(m => m.KillProcesses(It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.KillProcesses(It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();

            var options = TestOptions.Create();

            _runnerService.Kill(options);

            _nodeServiceMock.Verify(m => m.KillProcesses(options), Times.Once);
            _dotnetServiceMock.Verify(m => m.KillProcesses(options), Times.Once);
        }

        [Fact]
        public void List_prints_all_apps_and_tests()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
               .Returns(SampleProjects.ConfigurationGood)
               .Verifiable();

            var output = _runnerService.List(TestOptions.Create());

            Assert.Equal(SampleProjects.ConfigurationGood.Apps.Count, output.Apps.Count);
            Assert.Equal(SampleProjects.ConfigurationGood.Tests.Count, output.Tests.Count);

            Assert.Contains("Apps:", _console.LogOutput);
            Assert.Contains("Tests:", _console.LogOutput);
        }

        [Theory]
        [InlineData("node", 1, 0)]
        [InlineData("test", 0, 2)]
        [InlineData("SampleApp1", 1, 0)]
        public void List_prints_only_apps_or_tests_that_match_name_if_specified(string name, int appCount, int testCount)
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
               .Returns(SampleProjects.ConfigurationGood)
               .Verifiable();

            var output = _runnerService.List(new RegiOptions { Name = name });

            Assert.Equal(appCount, output.Apps.Count);
            Assert.Equal(testCount, output.Tests.Count);

            if (appCount <= 0)
                Assert.DoesNotContain("Apps:", _console.LogOutput);
            if (testCount <= 0)
                Assert.DoesNotContain("Tests:", _console.LogOutput);
        }
    }
}
