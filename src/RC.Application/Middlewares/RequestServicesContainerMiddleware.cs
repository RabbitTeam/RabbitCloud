using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Application.Middlewares
{
    public class RequestServicesContainerMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public RequestServicesContainerMiddleware(RabbitRequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public async Task Invoke(IRabbitContext rabbitContext)
        {
            // local cache for virtual disptach result
            var features = rabbitContext.Features;
            var existingFeature = features.Get<IServiceProvidersFeature>();

            // All done if RequestServices is set
            if (existingFeature?.RequestServices != null)
            {
                await _next.Invoke(rabbitContext);
                return;
            }

            var replacementFeature = new RequestServicesFeature(_scopeFactory);

            try
            {
                features.Set<IServiceProvidersFeature>(replacementFeature);
                await _next.Invoke(rabbitContext);
            }
            finally
            {
                replacementFeature.Dispose();
                // Restore previous feature state
                features.Set(existingFeature);
            }
        }
    }
}