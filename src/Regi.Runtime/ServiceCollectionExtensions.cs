using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Regi.Abstractions;

namespace Regi.Runtime
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRegiRuntime(this IServiceCollection services)
        {
            services.TryAddScoped<IProcessManager, ProcessManager>();
            services.TryAddScoped<ILogSinkManager, LogSinkManager>();
            services.TryAddScoped<ILogHandlerFactory, LogHandlerFactory>();

            return services;
        }
    }
}
