using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Regi.CommandLine;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class FrameworkServiceProviderTests
    {
        private readonly TestConsole _console;

        public FrameworkServiceProviderTests(ITestOutputHelper outputHelper)
        {
            _console = new TestConsole(outputHelper);
        }

        private IFrameworkServiceProvider CreateService(IServiceProvider provider = null)
        {
            if (provider == null)
            {
                provider = Program.ConfigureServices(_console);
            }

            return new FrameworkServiceProvider(provider);
        }

        [Fact]
        public void Returns_framework_service_for_every_ProjectFramework_except_Any()
        {
            var service = CreateService();

            foreach (var enumValue in Enum.GetValues(typeof(ProjectFramework)))
            {
                ProjectFramework projectFramework = (ProjectFramework)enumValue;

                if (projectFramework != ProjectFramework.Any)
                {
                    try
                    {
                        Assert.IsAssignableFrom<IFramework>(service.GetFrameworkService(projectFramework));
                    }
                    catch (Exception e)
                    {
                        _console.WriteLine($"Failed to get framework service for framework type {projectFramework}. Details:\n{e}");

                        throw;
                    }
                }
            }
        }

        [Fact]
        public void Throws_when_trying_to_get_Any_framework_service()
        {
            var service = CreateService();

            Assert.Throws<ArgumentException>(() => service.GetFrameworkService(ProjectFramework.Any));
        }

        [Fact]
        public void Throws_when_trying_to_get_a_not_implemented_framework_type()
        {
            var service = CreateService();

            Assert.Throws<NotImplementedException>(() => service.GetFrameworkService(0));
        }

        [Fact]
        public void Throws_if_framework_service_has_not_been_registered_with_service_provider()
        {
            var emptyServiceProvider = new ServiceCollection().BuildServiceProvider();

            var service = CreateService(emptyServiceProvider);

            Assert.Throws<InvalidOperationException>(() => service.GetFrameworkService(ProjectFramework.Dotnet));
        }

        [Fact]
        public void GetAllProjectFrameworkTypes_returns_every_type_of_project_framework_except_Any()
        {
            var service = CreateService(null);

            var allProjectFrameworkTypes = service.GetAllProjectFrameworkTypes();

            foreach (var enumValue in Enum.GetValues(typeof(ProjectFramework)))
            {
                ProjectFramework projectFramework = (ProjectFramework)enumValue;

                if (projectFramework != ProjectFramework.Any)
                {
                    Assert.Contains(projectFramework, allProjectFrameworkTypes);
                    allProjectFrameworkTypes.Remove(projectFramework);
                }
            }

            Assert.Empty(allProjectFrameworkTypes);
        }
    }
}
