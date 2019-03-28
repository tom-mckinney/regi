using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    internal class WumboService : FrameworkService
    {
        public WumboService(IConsole console) : base(console, "wumbo") {}

        protected override ProjectOptions FrameworkOptions => new ProjectOptions
        {
            { "super-wumbo", new List<string> { "--do-the-thing" } }
        };

        protected override string FormatAdditionalArguments(string args)
        {
            return $"***{args}***";
        }

        public override AppProcess InstallProject(Project project, CommandOptions options)
        {
            throw new NotImplementedException();
        }

        public override AppProcess StartProject(Project project, CommandOptions options)
        {
            throw new NotImplementedException();
        }

        public override AppProcess TestProject(Project project, CommandOptions options)
        {
            throw new NotImplementedException();
        }

        protected override void SetEnvironmentVariables(Process process, Project project)
        {
            throw new NotImplementedException();
        }
    }

    public class FrameworkServiceTests
    {
        private readonly IConsole _console;

        public FrameworkServiceTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
        }

        [Fact]
        public void BuildCommand_returns_default_command_by_default()
        {
            var service = new WumboService(_console);

            Assert.Equal("foo", service.BuildCommand("foo", null, null));
        }

        [Fact]
        public void BuildCommand_returns_command_if_project_overrides_default_command()
        {
            var service = new WumboService(_console);

            var project = new Project
            {
                Commands = new Dictionary<string, string> { { "start", "wumbo" } }
            };

            Assert.Equal("wumbo", service.BuildCommand("start", project, null));
        }

        [Fact]
        public void BuildCommand_joins_all_options_if_specified_with_wildcards()
        {
            var service = new WumboService(_console);

            var project = new Project
            {
                Options = new ProjectOptions
                {
                    { "*", new List<string> { "-v", "--runtime ubuntu.18.04-x64" } }
                }
            };

            Assert.Equal("foo -v --runtime ubuntu.18.04-x64", service.BuildCommand("foo", project, null));
        }

        [Fact]
        public void BuildCommand_joins_any_options_where_key_matches_the_command()
        {
            var service = new WumboService(_console);

            var project = new Project
            {
                Options = new ProjectOptions
                {
                    { "*", new List<string> { "--wumbo" } },
                    { "foo", new List<string> { "--t bar" } },
                    { "bar", new List<string> { "--t foo" } }
                }
            };

            Assert.Equal("foo --wumbo --t bar", service.BuildCommand("foo", project, null));
        }

        [Fact]
        public void BuildCommand_applys_any_framework_default_options_for_command()
        {
            var service = new WumboService(_console);

            Assert.Equal("super-wumbo --do-the-thing", service.BuildCommand("super-wumbo", null, null));
        }

        [Fact]
        public void BuildCommand_formats_additional_arguments_custom_format_if_overwritten()
        {
            var service = new WumboService(_console);

            var options = new CommandOptions
            {
                Arguments = "very good wumbo"
            };

            Assert.Equal("foo ***very good wumbo***", service.BuildCommand("foo", null, options));
        }
    }
}
