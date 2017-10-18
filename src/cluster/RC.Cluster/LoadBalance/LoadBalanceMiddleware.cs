using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Cluster.Abstractions.LoadBalance;
using Rabbit.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Cluster.LoadBalance
{
    public class LoadBalanceMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ILogger<LoadBalanceMiddleware> _logger;
        private readonly LoadBalanceOptions _options;

        public LoadBalanceMiddleware(RabbitRequestDelegate next, ILogger<LoadBalanceMiddleware> logger, LoadBalanceOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options;
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
                var serviceInstanceChoose = GetChoose(context);
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

        private IServiceInstanceChoose GetChoose(IRabbitContext context)
        {
            IServiceInstanceChoose GetChoose(string strategy, bool rquired)
            {
                var services = context.RequestServices;
                return rquired ? services.GetRequiredNamedService<IServiceInstanceChoose>(strategy) : services.GetNamedService<IServiceInstanceChoose>(strategy);
            }
            if (string.IsNullOrEmpty(_options.StrategyName))
            {
            }
            else
            {
                return context.RequestServices.GetRequiredNamedService<IServiceInstanceChoose>(_options.StrategyName);
            }
            return GetChoosers(context).FirstOrDefault(i => i != null) ?? context.RequestServices.GetRequiredService<IServiceInstanceChoose>();
        }

        private IEnumerable<IServiceInstanceChoose> GetChoosers(IRabbitContext context)
        {
            return GetChooserNames(context).Where(i => !string.IsNullOrEmpty(i)).Select(name => context.RequestServices.GetRequiredNamedService<IServiceInstanceChoose>(name));
        }

        private IEnumerable<string> GetChooserNames(IRabbitContext context)
        {
            yield return _options.StrategyName;

            var request = context.Request;
            var query = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);

            const string key = LoadBalanceConstants.ChooserKey;

            var items = query?.FirstOrDefault(i => key.Equals(i.Key, StringComparison.OrdinalIgnoreCase)).Value ?? StringValues.Empty;
            if (!StringValues.IsNullOrEmpty(items))
                foreach (var value in items)
                    yield return value;

            items = request.Headers.FirstOrDefault(i => key.Equals(i.Key, StringComparison.OrdinalIgnoreCase)).Value;
            if (StringValues.IsNullOrEmpty(items)) yield break;
            foreach (var value in items)
                yield return value;
        }
    }
}