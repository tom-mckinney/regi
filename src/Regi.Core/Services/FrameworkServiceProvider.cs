using Microsoft.Extensions.DependencyInjection;
using Regi.Frameworks;
using Regi.Models;
using System;
using System.Collections.Generic;

namespace Regi.Services
{
    public interface IFrameworkServiceProvider
    {
        IFramework GetFrameworkService(ProjectFramework framework);
        IQueueService CreateScopedQueueService();
        IList<ProjectFramework> GetAllProjectFrameworkTypes();
    }

    public class FrameworkServiceProvider : IFrameworkServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public FrameworkServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFramework GetFrameworkService(ProjectFramework framework)
        {
            return framework switch
            {
                ProjectFramework.Dotnet => _serviceProvider.GetRequiredService<IDotnet>(),
                ProjectFramework.Node => _serviceProvider.GetRequiredService<INode>(),
                ProjectFramework.RubyOnRails => _serviceProvider.GetRequiredService<IRubyOnRails>(),
                ProjectFramework.Any => throw new ArgumentException($"Cannot get framework service for project framework type of {framework}", nameof(framework)),
                _ => throw new NotImplementedException($"There is no implementation for framework of type {framework}"),
            };
        }

        public IQueueService CreateScopedQueueService()
        {
            using var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IQueueService>();
        }

        public IList<ProjectFramework> GetAllProjectFrameworkTypes()
        {
            return new List<ProjectFramework>
            {
                ProjectFramework.Dotnet,
                ProjectFramework.Node,
                ProjectFramework.RubyOnRails,
            };
        }
    }
}
