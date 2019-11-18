using Moq;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test
{
    public class ProjectManagerTests : TestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ICleanupService> _cleanupServiceMock = new Mock<ICleanupService>();

        public ProjectManagerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public IProjectManager CreateProjectManager()
        {
            return new ProjectManager(new TestConsole(_output), _cleanupServiceMock.Object);
        }

        [Fact]
        public void FilterByOptions_excludes_optional_projects_by_default()
        {
            var optionalProject = SampleProjects.Backend;

            optionalProject.Optional = true;

            var projectManager = CreateProjectManager();

            var filteredProjects = projectManager.FilterByOptions(new List<Project> { optionalProject }, TestOptions.Create());

            Assert.Empty(filteredProjects);
        }

        [Fact]
        public void FilterByOptions_includes_optional_if_include_optional_is_specified()
        {
            var optionalProject = SampleProjects.Backend;

            optionalProject.Optional = true;

            var projectManager = CreateProjectManager();

            var options = TestOptions.Create();

            options.IncludeOptional = true;

            var filteredProjects = projectManager.FilterByOptions(new List<Project> { optionalProject }, options);

            var p = Assert.Single(filteredProjects);
            Assert.Same(optionalProject, p);
        }
    }
}
