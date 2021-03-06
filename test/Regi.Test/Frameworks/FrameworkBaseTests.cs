﻿using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Frameworks;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Frameworks
{
    public class FrameworkBaseTests
    {
        private readonly TestFileSystem _fileSystem = new TestFileSystem();
        private readonly IConsole _console;

        public FrameworkBaseTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
        }

        [Fact]
        public void BuildCommand_returns_default_command_by_default()
        {
            var service = new WumboService(_console);

            Assert.Equal("foo", service.BuildCommandArguments("foo", null, null));
        }

        [Fact]
        public void BuildCommand_returns_command_if_project_overrides_default_command()
        {
            var service = new WumboService(_console);

            var project = new Project
            {
                Commands = new Dictionary<string, string> { { "start", "wumbo" } }
            };

            Assert.Equal("wumbo", service.BuildCommandArguments("start", project, null));
        }

        [Fact]
        public void BuildCommand_joins_all_options_if_specified_with_wildcards()
        {
            var service = new WumboService(_console);

            var project = new Project
            {
                Arguments = new CommandDictionary
                {
                    { "*", new List<string> { "-v", "--runtime ubuntu.18.04-x64" } }
                }
            };

            Assert.Equal("foo -v --runtime ubuntu.18.04-x64", service.BuildCommandArguments("foo", project, null));
        }

        [Fact]
        public void BuildCommand_joins_any_options_where_key_matches_the_command()
        {
            var service = new WumboService(_console);

            var project = new Project
            {
                Arguments = new CommandDictionary
                {
                    { "*", new List<string> { "--wumbo" } },
                    { "foo", new List<string> { "--t bar" } },
                    { "bar", new List<string> { "--t foo" } }
                }
            };

            Assert.Equal("foo --wumbo --t bar", service.BuildCommandArguments("foo", project, null));
        }

        [Fact]
        public void BuildCommand_applys_any_framework_default_options_for_command()
        {
            var service = new WumboService(_console);

            Assert.Equal("super-wumbo --do-the-thing", service.BuildCommandArguments("super-wumbo", null, null));
        }

        [Fact]
        public void BuildCommand_formats_additional_arguments_custom_format_if_overwritten()
        {
            var service = new WumboService(_console);

            var options = new CommandOptions
            {
                RemainingArguments = new List<string> {"--wumbo good", "--verbose"}
            };

            Assert.Equal("foo ***--wumbo good --verbose***", service.BuildCommandArguments("foo", null, options));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateProcess_handles_error_data_regardless_of_verbosity(bool isVerbose)
        {
            var service = new WumboService(_console);

            var options = TestOptions.Create();

            options.Verbose = isVerbose;

            var process = service.CreateProcess(FrameworkCommands.DotnetCore.Run, SampleProjects.Backend, SampleProjects.Backend.GetAppDirectoryPaths(_fileSystem)[0], options, "dotnet");

            Assert.True(process.ErrorDataHandled);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateProcess_handles_output_data_only_in_verbose_mode(bool isVerbose)
        {
            var service = new WumboService(_console);

            var options = TestOptions.Create();

            options.Verbose = isVerbose;

            var process = service.CreateProcess(FrameworkCommands.DotnetCore.Run, SampleProjects.Backend, SampleProjects.Backend.GetAppDirectoryPaths(_fileSystem)[0], options, "dotnet");

            if (isVerbose)
                Assert.True(process.OutputDataHandled);
            else
                Assert.False(process.OutputDataHandled);
        }

        [Fact]
        public void CreateProcess_handles_output_data_for_project_when_in_show_output_options()
        {
            var service = new WumboService(_console);

            var options = TestOptions.Create();

            options.Verbose = false;

            options.ShowOutput = new List<string> { SampleProjects.Backend.Name };            

            var process = service.CreateProcess(FrameworkCommands.DotnetCore.Run, SampleProjects.Backend, SampleProjects.Backend.GetAppDirectoryPaths(_fileSystem)[0], options, "dotnet");

            Assert.True(process.OutputDataHandled);
        }

        [Fact]
        public void CreateProcess_does_not_handle_output_data_if_project_is_not_in_show_output()
        {
            var service = new WumboService(_console);

            var options = TestOptions.Create();

            options.Verbose = false;

            options.ShowOutput = new List<string> { "Wumbo" };

            var process = service.CreateProcess(FrameworkCommands.DotnetCore.Run, SampleProjects.Backend, SampleProjects.Backend.GetAppDirectoryPaths(_fileSystem)[0], options, "dotnet");

            Assert.False(process.OutputDataHandled);
        }
    }

    internal class WumboService : FrameworkBase
    {

        public WumboService(IConsole console) : base(console, new PlatformService(new RuntimeInfo(), new TestFileSystem(), console), "wumbo") { }

        public override ProjectFramework Framework => throw new NotImplementedException();
        public override IEnumerable<string> ProcessNames => throw new NotImplementedException();

        protected override CommandDictionary FrameworkOptions => new CommandDictionary
        {
            { "super-wumbo", new List<string> { "--do-the-thing" } }
        };

        protected override string FormatAdditionalArguments(IEnumerable<string> args)
        {
            return $"***{string.Join(' ', args)}***";
        }
    }
}
