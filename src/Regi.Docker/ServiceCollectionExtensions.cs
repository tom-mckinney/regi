using Microsoft.Extensions.DependencyInjection;
using Regi.Abstractions;

namespace Regi.Docker
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDocker(this IServiceCollection services)
        {
            services.AddScoped<IServiceRunner, DockerServiceRunner>();

            return services;
        }
    }
}
