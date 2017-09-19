using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RC.Abstractions;
using RC.Cluster.Abstractions.LoadBalance;
using System;
using System.Threading.Tasks;

namespace RC.Cluster.LoadBalance
{
    public class LoadBalanceMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ILogger<LoadBalanceMiddleware> _logger;

        public LoadBalanceMiddleware(RabbitRequestDelegate next, ILogger<LoadBalanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(RabbitContext context)
        {
            var request = context.Request.RequestMessage;
            var current = request.RequestUri;
            if (!current.IsDefaultPort)
            {
                await _next(context);
                return;
            }

            try
            {
                var uri = await LookupServiceAsync(current,
                    context.RequestServices.GetRequiredService<IServiceInstanceChoose>());
                request.RequestUri = new Uri(uri, request.RequestUri.PathAndQuery);
                await _next(context);
            }
            finally
            {
                request.RequestUri = current;
            }
        }

        private async Task<Uri> LookupServiceAsync(Uri current, IServiceInstanceChoose serviceInstanceChoose)
        {
            _logger?.LogDebug("LookupService({0})", current.ToString());

            var serviceName = current.Host;
            var instance = await serviceInstanceChoose.ChooseAsync(serviceName);
            if (instance == null)
            {
                _logger.LogWarning($"basis serviceName '{serviceName}' not found serviceInstance.");
            }
            else
                current = instance.Uri;

            _logger?.LogDebug("LookupService() returning {0} ", current.ToString());
            return current;
        }
    }
}