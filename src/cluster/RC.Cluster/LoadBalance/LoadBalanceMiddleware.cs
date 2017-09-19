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
                    context.RequestServices.GetRequiredService<IAddressSelector>());
                request.RequestUri = new Uri(uri, request.RequestUri.PathAndQuery);
                await _next(context);
            }
            finally
            {
                request.RequestUri = current;
            }
        }

        private async Task<Uri> LookupServiceAsync(Uri current, IAddressSelector addressSelector)
        {
            _logger?.LogDebug("LookupService({0})", current.ToString());

            current = await addressSelector.SelectAsync(current.Host);

            _logger?.LogDebug("LookupService() returning {0} ", current.ToString());
            return current;
        }
    }
}