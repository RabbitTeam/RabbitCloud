using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.LoadBalance.Features;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public class LoadBalanceConfigurationOptions
    {
        public class LoadBalanceConfigurationItem
        {
            public string Chooser { get; set; }
            public int MaxAutoRetries { get; set; }
            public int MaxAutoRetriesNextServer { get; set; }
        }

        public IDictionary<string, LoadBalanceConfigurationItem> ConfigurationItems { get; } = new Dictionary<string, LoadBalanceConfigurationItem>();
    }

    public class LoadBalanceConfigurationMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IServiceInstanceChooserCollection _serviceInstanceChooserCollection;
        private readonly LoadBalanceConfigurationOptions _options;

        public LoadBalanceConfigurationMiddleware(RabbitRequestDelegate next, IOptions<LoadBalanceConfigurationOptions> options, IOptions<LoadBalanceOptions> loadBalanceOptions)
        {
            _next = next;
            _serviceInstanceChooserCollection = loadBalanceOptions.Value.ServiceInstanceChooserCollection;
            _options = options.Value;
        }

        private void SetLoadBalanceFeature(ILoadBalanceFeature loadBalanceFeature, LoadBalanceConfigurationOptions.LoadBalanceConfigurationItem item)
        {
            SetLoadBalanceFeature(loadBalanceFeature, item.Chooser, item.MaxAutoRetries, item.MaxAutoRetriesNextServer);
        }

        private void SetLoadBalanceFeature(ILoadBalanceFeature loadBalanceFeature, string chooser, int maxAutoRetries, int maxAutoRetriesNextServer)
        {
            if (loadBalanceFeature.ServiceInstanceChooser == null)
            {
                loadBalanceFeature.ServiceInstanceChooser = _serviceInstanceChooserCollection.Get(chooser);
            }
            if (loadBalanceFeature.Strategy == null)
                loadBalanceFeature.Strategy = new LoadBalanceStrategy();

            if (loadBalanceFeature.Strategy.MaxAutoRetries <= 0)
            {
                loadBalanceFeature.Strategy.MaxAutoRetries = maxAutoRetries;
            }
            if (loadBalanceFeature.Strategy.MaxAutoRetriesNextServer <= 0)
            {
                loadBalanceFeature.Strategy.MaxAutoRetriesNextServer = maxAutoRetriesNextServer;
            }
        }

        public async Task Invoke(IRabbitContext context)
        {
            var serviceId = context.Request.Url.Host;
            ILoadBalanceFeature loadBalanceFeature = new LoadBalanceFeature();
            context.Features.Set(loadBalanceFeature);

            // first service configuration
            if (_options.ConfigurationItems.TryGetValue(serviceId, out var item))
                SetLoadBalanceFeature(loadBalanceFeature, item);

            // second default configuration
            if (_options.ConfigurationItems.TryGetValue("Default", out item))
                SetLoadBalanceFeature(loadBalanceFeature, item);

            // ensure safety
            SetLoadBalanceFeature(loadBalanceFeature, "Random", 3, 1);

            await _next(context);
        }
    }
}