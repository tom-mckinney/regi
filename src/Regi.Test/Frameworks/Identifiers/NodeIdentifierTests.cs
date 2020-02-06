using Regi.Frameworks.Identifiers;
using Regi.Models;
using System.Threading.Tasks;
using Xunit;

namespace Regi.Test.Identifiers
{
    public class NodeIdentifierTests : BaseIdentifierTests
    {
        protected override IIdentifier CreateTestClass()
        {
            return new NodeIdentifier(_fileSystemMock.Object);
        }

        protected override void ShouldHaveMatched(Project expectedProject, bool wasMatch)
        {
            if (expectedProject.Framework == ProjectFramework.Node)
            {
                Assert.True(wasMatch);
            }
            else
            {
                Assert.False(wasMatch);
            }
        }

        [Theory]
        [MemberData(nameof(NodeProjects))]
        public async Task Identify_node_project(string name, Project expectedProject)
        {
            var actualProject = await Identify_base_project(name, expectedProject);

            Assert.Equal(ProjectFramework.Node, actualProject.Framework);

            // TODO: test for type
        }
    }
}
