using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Cluster.Abstractions.LoadBalance;
using Rabbit.Cloud.Cluster.LoadBalance.Features;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Cluster.LoadBalance
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

        public async Task Invoke(IRabbitContext context)
        {
            var request = context.Request;
            var current = request.RequestUri;
            if (!current.IsDefaultPort)
            {
                await _next(context);
                return;
            }

            try
            {
                var serviceInstanceChoose = context.Features.Get<ILoadBalanceFeature>().ServiceInstanceChoose;
                var uri = await LookupServiceAsync(current, serviceInstanceChoose);
                request.RequestUri = new Uri(uri, request.RequestUri.PathAndQuery);
                //todo whether handle Query?
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