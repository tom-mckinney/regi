using Regi.Frameworks.Identifiers;
using Regi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Regi.Test.Identifiers
{
    public class DotnetIdentifierTests : BaseIdentifierTests
    {
        protected override IIdentifier CreateTestClass()
        {
            return new DotnetIdentifier(FileSystemMock.Object);
        }

        protected override void ShouldHaveMatched(Project expectedProject, bool wasMatch)
        {
            if (expectedProject.Framework == ProjectFramework.Dotnet)
            {
                Assert.True(wasMatch);
            }
            else
            {
                Assert.False(wasMatch);
            }
        }

        [Theory]
        [MemberData(nameof(DotnetProjects))]
        public async Task Identify_dotnet_project(string name, Project expectedProject)
        {
            var actualProject = await Identify_base_project(name, expectedProject);

            Assert.Equal(ProjectFramework.Dotnet, actualProject.Framework);

            Assert.Equal(expectedProject.Roles, actualProject.Roles);

            switch (actualProject.Roles.Single())
            {
                case ProjectRole.App:
                    Assert.Equal(8080, actualProject.Port);
                    break;
                case ProjectRole.Test:
                    Assert.Null(actualProject.Port);
                    break;
                default:
                    throw new InvalidOperationException("Not a valid project type.");
            }
        }
    }
}
