using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Regi.Test.Utilities
{
    public class PathUtilityTests
    {
        [Theory]
        [InlineData("dotnet run test", "dotnet")]
        [InlineData("dotnet", "dotnet")]
        [InlineData("dotnet npm wumbo", "dotnet")]
        [InlineData("npm test", "npm")]
        [InlineData("npm", "npm")]
        [InlineData("npm dotnet wumbo", "npm")]
        public void GetFileNameFromScript_returns_expected_file_name(string script, string expectedFileName)
        {
            string actualFileName = PathUtility.GetFileNameFromCommand(script);

            Assert.Equal(expectedFileName, actualFileName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GetfileNameFromScript_throws_if_script_is_null_or_empty(string script)
        {
            Assert.Throws<ArgumentException>(() => PathUtility.GetFileNameFromCommand(script));
        }
    }
}
