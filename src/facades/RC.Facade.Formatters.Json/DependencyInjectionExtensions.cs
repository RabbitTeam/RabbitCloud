using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions;
using RC.Facade.Formatters.Json.Internal;
using System;

namespace RC.Facade.Formatters.Json
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddJsonFormatters(this IServiceCollection services,
            Action<FacadeJsonOptions> setupAction = null)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<FacadeOptions>, FacadeJsonFacadeOptionsSetup>());

            if (setupAction != null)
                services.Configure(setupAction);

            return services;
        }
    }
}