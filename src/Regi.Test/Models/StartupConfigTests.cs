using Regi.Models;
using System.Collections.Generic;
using Xunit;

namespace Regi.Test.Models
{
    public class StartupConfigTests
    {
        [Fact]
        public void Sources_throws_if_RawSources_has_invalid_enum_name()
        {
            var config = new StartupConfig
            {
                RawSources = new Dictionary<string, string>
                {
                    { "foo", "https://www.bar.com" }
                }
            };

            Assert.Throws<RegiException>(() => config.GetSources());
        }

        [Theory]
        [InlineData("dotnet")]
        [InlineData("Dotnet")]
        [InlineData("DotNet")]
        [InlineData("DOTNET")]
        public void Sources_allows_any_casing_for_enum_names(string framework)
        {
            var config = new StartupConfig
            {
                RawSources = new Dictionary<string, string>
                {
                    { framework, "https://www.bar.com" }
                }
            };

            var source = Assert.Single(config.GetSources());
            Assert.Equal(ProjectFramework.Dotnet, source.Key);
        }
    }
}
