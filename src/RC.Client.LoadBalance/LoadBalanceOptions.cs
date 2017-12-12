using Microsoft.Extensions.Configuration;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Client.LoadBalance.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public class LoadBalanceClientOptions
    {
        public IServiceInstanceChooserCollection ServiceInstanceChooserCollection { get; } = new ServiceInstanceChooserCollection();
        public IDictionary<string, RequestOptions> ServiceRequestOptions { get; set; } = new Dictionary<string, RequestOptions>(StringComparer.OrdinalIgnoreCase);

        internal void ApplyRequestOptions(IRabbitContext rabbitContext)
        {
            var service = rabbitContext.Request.Url.Host;

            IEnumerable<RequestOptions> GetOptions()
            {
                if (ServiceRequestOptions.TryGetValue(service, out var o))
                    yield return o;
                yield return RequestOptions.Default;
            }

            var options = GetOptions().First(i => i != null);
            options.Apply(rabbitContext);
        }

        public class RequestOptions
        {
            public TimeSpan ConnectionTimeout { get; set; }
            public TimeSpan ReadTimeout { get; set; }
            public IServiceInstanceChooser Chooser { get; set; }
            public int MaxAutoRetries { get; set; }
            public int MaxAutoRetriesNextServer { get; set; }

            public static RequestOptions Default { get; } = new RequestOptions
            {
                ConnectionTimeout = TimeSpan.FromSeconds(2),
                ReadTimeout = TimeSpan.FromSeconds(10),
                Chooser = new RandomServiceInstanceChooser(),
                MaxAutoRetries = 3,
                MaxAutoRetriesNextServer = 1
            };

            internal void Apply(IRabbitContext rabbitContext)
            {
                var requestFeature = rabbitContext.Features.GetOrAdd<IRequestFeature>(() => new RequestFeature());
                var loadBalanceFeature = rabbitContext.Features.GetOrAdd<ILoadBalanceFeature>(() => new LoadBalanceFeature());

                loadBalanceFeature.ServiceInstanceChooser = Chooser;

                if (loadBalanceFeature.Strategy == null)
                    loadBalanceFeature.Strategy = new LoadBalanceStrategy();
                loadBalanceFeature.Strategy.MaxAutoRetries = MaxAutoRetries;
                loadBalanceFeature.Strategy.MaxAutoRetriesNextServer = MaxAutoRetriesNextServer;

                requestFeature.ConnectionTimeout = ConnectionTimeout;
                requestFeature.ReadTimeout = ReadTimeout;
            }

            public static RequestOptions Create(IConfiguration configuration, LoadBalanceClientOptions options)
            {
                IEnumerable<IServiceInstanceChooser> GetChoosers()
                {
                    var chooserName = configuration[nameof(Chooser)];
                    if (!string.IsNullOrEmpty(chooserName))
                        yield return options.ServiceInstanceChooserCollection.Get(chooserName);
                    yield return Default.Chooser;
                }

                var requestOptions = new RequestOptions
                {
                    ConnectionTimeout =
                        TimeUtilities.GetTimeSpanBySimpleOrDefault(configuration[nameof(ConnectionTimeout)],
                            Default.ConnectionTimeout),
                    ReadTimeout =
                        TimeUtilities.GetTimeSpanBySimpleOrDefault(configuration[nameof(ReadTimeout)],
                            Default.ReadTimeout),
                    Chooser = GetChoosers().First(i => i != null),
                    MaxAutoRetries = configuration.GetValue<int?>(nameof(MaxAutoRetries)) ?? Default.MaxAutoRetries,
                    MaxAutoRetriesNextServer = configuration.GetValue<int?>(nameof(MaxAutoRetriesNextServer)) ?? Default.MaxAutoRetriesNextServer
                };

                return requestOptions;
            }
        }
    }
}