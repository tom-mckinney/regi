using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentifer<T>(this IServiceCollection services)
            where T : class, IIdentifier
        {
            return services.AddTransient<IIdentifier, T>();
        }
    }
}
