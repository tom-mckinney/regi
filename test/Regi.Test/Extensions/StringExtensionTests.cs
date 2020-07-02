using Regi.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Regi.Test.Extensions
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData("Project", "PROJECT")]
        [InlineData("TestProject", "TESTPROJECT")]
        [InlineData("Test Project", "TEST_PROJECT")]
        [InlineData("Test.Project", "TEST_PROJECT")]
        [InlineData("Test-Project", "TEST_PROJECT")]
        [InlineData("Test, Project", "TEST__PROJECT")]
        public void ToUnderscoreCase_converts_input_to_expected_output(string input, string output)
        {
            Assert.Equal(output, input.ToUnderscoreCase());
        }
    }
}
