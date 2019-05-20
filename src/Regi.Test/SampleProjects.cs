using Regi.Models;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Test
{
    public static class SampleProjects
    {
        public static Project Frontend = new Project
        {
            Name = "Frontend",
            Path = PathHelper.SampleDirectoryPath("Frontend/package.json"),
            Framework = ProjectFramework.Node,
            Port = 3000,
            Commands = new Dictionary<string, string>
            {
                { "start", "run dev" }
            }
        };

        public static Project Backend = new Project
        {
            Name = "Backend",
            Path = PathHelper.SampleDirectoryPath("Backend/Backend.csproj"),
            Framework = ProjectFramework.Dotnet,
            Port = 5000,
            Options = new ProjectOptions
            {
                { "*", new List<string> { "--foo bar" } }
            }
        };

        public static Project XunitTests = new Project
        {
            Name = "SampleSuccessfulTests",
            Path = PathHelper.SampleDirectoryPath("SampleSuccessfulTests/SampleSuccessfulTests.csproj"),
            Framework = ProjectFramework.Dotnet,
            Type = ProjectType.Unit
        };

        public static Project JestTests = new Project
        {
            Name = "NodeApp",
            Path = PathHelper.SampleDirectoryPath("SampleNodeApp/package.json"),
            Framework = ProjectFramework.Node
        };

        public static Project IntegrationTests = new Project
        {
            Name = "IntegrationTests",
            Path = PathHelper.SampleDirectoryPath("IntegrationTests/package.json"),
            Framework = ProjectFramework.Node,
            Requires = new List<string> { "Frontend", "Backend" },
            RawOutput = true,
            Serial = true,
            Port = 5000,
            Environment = new Dictionary<string, string>
            {
                { "HEADLESS", bool.FalseString }
            }
        };

        public static Project SimpleNodeApp = new Project
        {
            Name = "SampleNodeApp",
            Path = PathHelper.SampleDirectoryPath("SampleNodeApp/package.json"),
            Type = ProjectType.Web,
            Framework = ProjectFramework.Node,
            Port = 9081
        };

        public static StartupConfig ConfigurationDefault = new StartupConfig
        {
            Apps = new List<Project>
            {
                Frontend, Backend
            },
            Tests = new List<Project>
            {
                XunitTests, JestTests, IntegrationTests
            },
            Services = new List<Project>()
        };

        public static StartupConfig ConfigurationGood = new StartupConfig
        {
            Apps = new List<Project>
            {
                new Project
                {
                    Name = "SampleApp1",
                    Path = PathHelper.SampleDirectoryPath("SampleApp/SampleApp.csproj"),
                    Type = ProjectType.Web
                },
                new Project
                {
                    Name = "SampleApp2",
                    Path = PathHelper.SampleDirectoryPath("SampleApp/SampleApp.csproj"),
                    Type = ProjectType.Web,
                    Port = 9080,
                    Serial = true
                },
                SimpleNodeApp
            },
            Tests = new List<Project>
            {
                new Project
                {
                    Name = "SampleSuccessfulTests",
                    Path = PathHelper.SampleDirectoryPath("SampleSuccessfulTests/SampleSuccessfulTests.csproj"),
                    Type = ProjectType.Unit
                },
                new Project
                {
                    Name = "SampleSuccessfulTests",
                    Path = PathHelper.SampleDirectoryPath("SampleSuccessfulTests/SampleSuccessfulTests.csproj"),
                    Type = ProjectType.Integration,
                    Serial = true
                }
            },
            Services = new List<Project>(),
            Sources = new Dictionary<ProjectFramework, string>
            {
                { ProjectFramework.Dotnet, "http://nuget.org/api" },
                { ProjectFramework.Node, "http://npmjs.org" }
            }
        };
    }
}
