using Regi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Regi.Test.Models
{
    public class VariableListTests
    {
        [Fact]
        public void AddProject_includes_all_entries_in_Environment_property()
        {
            Project testProject = new Project
            {
                Environment = new Dictionary<string, string>
                {
                    { "FOO", "bar" }
                }
            };

            var variableList = new VariableList();

            variableList.AddProject(testProject);

            var targetVariable = Assert.Single(variableList);
            Assert.Equal("FOO", targetVariable.Key);
            Assert.Equal("bar", targetVariable.Value);
        }
    }
}
