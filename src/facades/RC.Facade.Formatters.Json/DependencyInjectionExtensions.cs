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
        public static IFacadeBuilder AddJsonFormatters(this IFacadeBuilder builder,
            Action<FacadeJsonOptions> setupAction = null)
        {
            var services = builder.Services;

            services.TryAddEnumerable(
                Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Transient<IConfigureOptions<FacadeOptions>, FacadeJsonFacadeOptionsSetup>());

            if (setupAction != null)
                services.Configure(setupAction);

            return builder;
        }
    }
}