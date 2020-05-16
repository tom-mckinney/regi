using Moq;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public void FilterByOptions_includes_all_standard_projects_by_default()
        {
            var projects = new[]
            {
                SampleProjects.Backend,
                SampleProjects.Frontend,
                SampleProjects.XunitTests,
                SampleProjects.IntegrationTests
            };

            var projectManager = CreateProjectManager();

            var options = TestOptions.Create();

            var filteredProjects = projectManager.FilterByOptions(projects, options);

            Assert.Equal(filteredProjects.Count, projects.Length);

            for (int i = 0; i < filteredProjects.Count; i++)
            {
                Assert.Same(filteredProjects[i], projects[i]);
            }
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

        [Theory]
        [InlineData(ProjectRole.Web)]
        [InlineData(ProjectRole.Unit)]
        public void FilterByOption_only_includes_roles_when_specified(params ProjectRole[] roles)
        {
            var appProject = SampleProjects.Backend;
            var testProject = SampleProjects.XunitTests;

            var projectManager = CreateProjectManager();

            var options = TestOptions.Create();

            options.Roles = roles;

            var filteredProjects = projectManager.FilterByOptions(new[] { appProject, testProject }, options);

            Assert.Single(filteredProjects, p => p.Roles.ContainsAll(roles));
            Assert.Empty(filteredProjects.Where(p => !p.Roles.ContainsAny(roles)));
        }
    }
}
