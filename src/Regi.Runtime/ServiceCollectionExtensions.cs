using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Regi.Abstractions;

namespace Regi.Runtime
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRegiRuntime(this IServiceCollection services)
        {
            services.TryAddScoped<IServiceRunnerDispatcher, ServiceRunnerDispatcher>();
            services.TryAddScoped<IProcessManager, ProcessManager>();
            services.TryAddScoped<ILogSinkManager, LogSinkManager>();
            services.TryAddScoped<ILogHandlerFactory, LogHandlerFactory>();
            
            services.AddLogging(builder =>
            {
                builder.AddConsole(options =>
                {
                    options.FormatterName = "Regi";
                });
                builder.AddConsoleFormatter<RegiConsoleFormatter, RegiConsoleFormatterOptions>();
            });

            return services;
        }
    }
}
