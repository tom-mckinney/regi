using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFramework<TFramework, TImplementation>(this IServiceCollection services)
            where TFramework : class, IFramework
            where TImplementation : class, TFramework

        {
            return services.AddTransient<TFramework, TImplementation>();
        }

        public static IServiceCollection AddIdentifer<T>(this IServiceCollection services)
            where T : class, IIdentifier
        {
            return services.AddTransient<IIdentifier, T>();
        }
    }
}
