using Regi.Abstractions;
using Regi.Models;
using Regi.Test.Helpers;
using System.Collections.Generic;

namespace Regi.Test
{
    public static class SampleProjects
    {
        public static Project ClassLib => new Project
        {
            Name = "ClassLib",
            Paths = new[] { PathHelper.GetSampleProjectPath("ClassLib") },
            Roles = new List<ProjectRole> { ProjectRole.App }, // TODO: have a classlib type
        };

        public static Project Frontend => new Project
        {
            Name = "Frontend",
            Path = PathHelper.GetSampleProjectPath("Frontend"),
            Framework = ProjectFramework.Node,
            Roles = new List<ProjectRole> { ProjectRole.App },
            Port = 3000,
            Commands = new Dictionary<string, string>
            {
                { "start", "run dev" }
            }
        };

        public static Project Backend => new Project
        {
            Name = "Backend",
            Path = PathHelper.GetSampleProjectPath("Backend"),
            Framework = ProjectFramework.Dotnet,
            Roles = new List<ProjectRole> { ProjectRole.App },
            Port = 5000,
            Arguments = new CommandDictionary
            {
                { "*", new List<string> { "--foo bar" } }
            }
        };

        public static Project Console => new Project
        {
            Name = "Console",
            Path = PathHelper.GetSampleProjectPath("SampleApp"),
            Framework = ProjectFramework.Dotnet,
            Roles = new List<ProjectRole> { ProjectRole.App },
        };

        public static Project ConsoleFailure => new Project
        {
            Name = "ConsoleFailure",
            Path = PathHelper.GetSampleProjectPath("SampleAppError"),
            Framework = ProjectFramework.Dotnet,
            Roles = new List<ProjectRole> { ProjectRole.App },
        };

        public static Project XunitTests => new Project
        {
            Name = "SampleSuccessfulTests",
            Path = PathHelper.GetSampleProjectPath("SampleSuccessfulTests"),
            Framework = ProjectFramework.Dotnet,
            Roles = new List<ProjectRole> { ProjectRole.Test }
        };

        public static Project XunitFailureTests => new Project
        {
            Name = "SampleFailedTests",
            Path = PathHelper.GetSampleProjectPath("SampleFailedTests"),
            Framework = ProjectFramework.Dotnet,
            Roles = new List<ProjectRole> { ProjectRole.Test }
        };

        public static Project JestTests => new Project
        {
            Name = "NodeApp",
            Path = PathHelper.GetSampleProjectPath("NodeApp/package.json"),
            Framework = ProjectFramework.Node
        };

        public static Project IntegrationTests => new Project
        {
            Name = "IntegrationTests",
            Path = PathHelper.GetSampleProjectPath("IntegrationTests/package.json"),
            Framework = ProjectFramework.Node,
            Requires = new List<string> { Frontend.Name, Backend.Name },
            RawOutput = true,
            Serial = true,
            Port = 5000,
            Environment = new Dictionary<string, object>
            {
                { "HEADLESS", bool.FalseString }
            }
        };

        public static Project AppCollection => new Project
        {
            Name = "AppCollection",
            Paths = new List<string>
            {
                PathHelper.GetSampleProjectPath("AppCollection/App1/App1.csproj"),
                PathHelper.GetSampleProjectPath("AppCollection/App2/App2.csproj"),
                PathHelper.GetSampleProjectPath("AppCollection/App3/App3.csproj"),
            },
            Framework = ProjectFramework.Dotnet,
            Roles = new List<ProjectRole> { ProjectRole.App }
        };

        public static Project TestCollection => new Project
        {
            Name = "TestCollection",
            Paths = new List<string>
            {
                PathHelper.GetSampleProjectPath("TestCollection/Test1/Test1.csproj"),
                PathHelper.GetSampleProjectPath("TestCollection/Test2/Test2.csproj"),
                PathHelper.GetSampleProjectPath("TestCollection/Test3/Test3.csproj"),
            },
            Framework = ProjectFramework.Dotnet,
            Roles = new List<ProjectRole> { ProjectRole.Test }
        };

        public static Project NodeApp => new Project
        {
            Name = "NodeApp",
            Path = PathHelper.GetSampleProjectPath("NodeApp/package.json"),
            Roles = new List<ProjectRole> { ProjectRole.App },
            Framework = ProjectFramework.Node,
            Port = 9081
        };

        public static IServiceMesh ConfigurationDefault => new RegiConfig
        {
            Projects = new List<IProject>
            {
                Frontend,
                Backend,
                XunitTests,
                JestTests,
                IntegrationTests,
            },
            Services = new List<IProject>()
        };

        public static IServiceMesh ConfigurationGood => new RegiConfig
        {
            Projects = new List<IProject>
            {
                new Project
                {
                    Name = "SampleApp1",
                    Path = PathHelper.GetSampleProjectPath("SampleApp/SampleApp.csproj"),
                    Roles = new List<ProjectRole> { ProjectRole.App }
                },
                new Project
                {
                    Name = "SampleApp2",
                    Path = PathHelper.GetSampleProjectPath("SampleApp/SampleApp.csproj"),
                    Roles = new List<ProjectRole> { ProjectRole.App },
                    Port = 9080,
                    Serial = true
                },
                NodeApp,
                new Project
                {
                    Name = "SampleSuccessfulTests",
                    Path = PathHelper.GetSampleProjectPath("SampleSuccessfulTests/SampleSuccessfulTests.csproj"),
                    Roles = new List<ProjectRole> { ProjectRole.Test }
                },
                new Project
                {
                    Name = "SampleSuccessfulTests",
                    Path = PathHelper.GetSampleProjectPath("SampleSuccessfulTests/SampleSuccessfulTests.csproj"),
                    Roles = new List<ProjectRole> { ProjectRole.Test },
                    Serial = true
                }
            },
            Services = new List<IProject>(),
            RawSources = new Dictionary<string, string>
            {
                { ProjectFramework.Dotnet.ToString(), "http://nuget.org/api" },
                { ProjectFramework.Node.ToString(), "http://npmjs.org" }
            }
        };
    }
}
