using Regi.Abstractions;
using Regi.Models;
using Regi.Services;
using System;
using Xunit;

namespace Regi.Test.Services
{
    public class ProjectFilterTests
    {
        private static IProjectFilter TestClass => new ProjectFilter();

        [Theory]
        [InlineData("Foo", "Foo", true)]
        [InlineData("Foo", "f", true)]
        [InlineData("Foo", "oo", true)]
        [InlineData("Foo", "Bar", false)]
        public void FilterByName_returns_projects_with_a_name_that_matches_regex_pattern(string projectName, string namePattern, bool isMatch)
        {
            var project = new Project { Name = projectName };

            var filteredProjects = TestClass.FilterByName(new[] { project }, namePattern);

            try
            {
                if (isMatch)
                {
                    Assert.NotEmpty(filteredProjects);
                }
                else
                {
                    Assert.Empty(filteredProjects);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Expected isMatch to equal {isMatch} for name {projectName} and pattern {namePattern}. Recieved {isMatch}", e);
            }
        }

        [Theory]
        [InlineData("e2e", "e2e", true)]
        [InlineData("e2e", "E2E", true)]
        [InlineData("e2e", "e", true)]
        [InlineData("e2e", "2", true)]
        [InlineData("unit-test", "unit-test", true)]
        [InlineData("unit-test", "unit", true)]
        [InlineData("unit-test", "test", true)]
        [InlineData("e2e", "unit-test", false)]
        public void FilterByLabels_returns_projects_that_contain_a_label_matching_one_of_regex_patterns(string label, string labelPattern, bool isMatch)
        {
            var project = new Project { Labels = new[] { label } };

            var filteredProjects = TestClass.FilterByLabels(new[] { project }, new[] { labelPattern } );

            try
            {
                if (isMatch)
                {
                    Assert.NotEmpty(filteredProjects);
                }
                else
                {
                    Assert.Empty(filteredProjects);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Expected isMatch to equal {isMatch} for label {label} and pattern {labelPattern}. Recieved {isMatch}", e);
            }
        }

        [Theory]
        [InlineData("e2e", "e2e", true)]
        [InlineData("e2e", "E2E", true)]
        [InlineData("e2e", "e", true)]
        [InlineData("e2e", "2", true)]
        [InlineData("unit-test", "unit-test", true)]
        [InlineData("unit-test", "unit", true)]
        [InlineData("unit-test", "test", true)]
        [InlineData("e2e", "unit-test", false)]
        public void FilterByExclusions_excludes_projects_with_a_name_matching_one_of_regex_patterns(string projectName, string exclusionPattern, bool isMatch)
        {
            var project = new Project { Name = projectName };

            var filteredProjects = TestClass.FilterByExclusions(new[] { project }, new[] { exclusionPattern });

            try
            {
                if (isMatch)
                {
                    Assert.Empty(filteredProjects); // invert isMatch because this method excludes matches
                }
                else
                {
                    Assert.NotEmpty(filteredProjects);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Expected isMatch to equal {!isMatch} for name {projectName} and exclusion pattern {exclusionPattern}. Recieved {isMatch}", e);
            }
        }

        [Theory]
        [InlineData(ProjectRole.App, ProjectRole.App, true)]
        [InlineData(ProjectRole.Test, ProjectRole.Test, true)]
        [InlineData(ProjectRole.App, ProjectRole.Test, false)]
        public void FilterByRoles_returns_projects_that_contain_a_role_equalling_one_of_roles(ProjectRole role, ProjectRole roleFilter, bool isMatch)
        {
            var project = new Project { Roles = new[] { role } };

            var filteredProjects = TestClass.FilterByRoles(new[] { project }, new[] { roleFilter });

            try
            {
                if (isMatch)
                {
                    Assert.NotEmpty(filteredProjects);
                }
                else
                {
                    Assert.Empty(filteredProjects);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Expected isMatch to equal {isMatch} for role {role} and role filter {roleFilter}. Recieved {isMatch}", e);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void FilterByOptional_returns_projects_that_are_not_optional(bool isOptional, bool isEmpty)
        {
            var project = new Project { Optional = isOptional };

            var filteredProjects = TestClass.FilterByOptional(new[] { project });

            try
            {
                if (isEmpty)
                {
                    Assert.Empty(filteredProjects);
                }
                else
                {
                    Assert.NotEmpty(filteredProjects);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Expected isEmpty to equal {isEmpty} for isOptional {isOptional}.", e);
            }
        }
    }
}
