using Microsoft.Extensions.DependencyInjection;
using Regi.CommandLine;
using Regi.Frameworks;
using Regi.Models;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Regi.Test
{
    public class FrameworkConfigurationTests
    {
        private readonly TestConsole _console;
        private readonly IServiceProvider _serviceProvider;
        private readonly Type[] _types;
        private readonly Type _iFrameworkType;

        public FrameworkConfigurationTests(ITestOutputHelper outputHelper)
        {
            _console = new TestConsole(outputHelper);
            _serviceProvider = Program.ConfigureServices(_console);
            _types = Assembly.Load("Regi.Core").GetTypes();
            _iFrameworkType = typeof(IFramework);
        }

        [Fact]
        public void All_Frameworks_are_properly_configured()
        {
            List<Exception> exceptions = new List<Exception>();

            ForeachFramework(type =>
            {
                try
                {
                    var hasEnum = Enum.TryParse(type.Name, out ProjectFramework projectFramework);
                    if (!hasEnum)
                    {
                        var msg = $"{type.Name} is not mapped to a ProjectFramework:\n";
                        msg += $"Must be one of: {string.Join(", ", typeof(ProjectFramework).GetEnumNames())}.";

                        throw new XunitException(msg);
                    }

                    var frameworkInterface = GetFrameworkInterface(type);

                    var framework = (IFramework)_serviceProvider.GetRequiredService(frameworkInterface);

                    Assert.Equal(projectFramework, framework.Framework);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            });

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private Type GetFrameworkInterface(Type frameworkType)
        {
            var frameworkInterface = frameworkType.GetInterfaces()
                .SingleOrDefault(t => 
                        _iFrameworkType.IsAssignableFrom(t)
                        && t != _iFrameworkType
                        && t.Name == $"I{frameworkType.Name}");

            if (frameworkInterface == null)
            {
                var msg = $"{frameworkType.Name} does not implement I{frameworkType.Name}. Every Framework must also have its own interface for tagging and testing purposes.";
                throw new XunitException(msg);
            }

            return frameworkInterface;
        }

        private void ForeachFramework(Action<Type> action)
        {
            foreach (var type in _types)
            {
                if (type.IsClass
                    && _iFrameworkType.IsAssignableFrom(type)
                    && type != typeof(FrameworkBase))
                {
                    action(type);
                }
            }
        }
    }
}
