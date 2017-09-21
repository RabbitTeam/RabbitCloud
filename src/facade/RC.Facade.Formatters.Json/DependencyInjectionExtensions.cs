using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Formatters.Json.Internal;
using System;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Rabbit.Cloud.Facade.Formatters.Json
{
    public static class DependencyInjectionExtensions
    {
        public static IFacadeBuilder AddJsonFormatters(this IFacadeBuilder builder,
            Action<FacadeJsonOptions> setupAction = null)
        {
            var services = builder.Services;

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<FacadeOptions>, FacadeJsonFacadeOptionsSetup>());

            if (setupAction != null)
                services.Configure(setupAction);

            return builder;
        }
    }
}