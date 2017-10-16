using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Cluster.Abstractions.LoadBalance;
using Rabbit.Cloud.Cluster.LoadBalance.Features;
using Rabbit.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Cluster.LoadBalance
{
    public class ServiceInstanceChooseMiddleware
    {
        private readonly RabbitRequestDelegate _next;

        public ServiceInstanceChooseMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var serviceInstanceChoose = GetChoosers(context).FirstOrDefault(i => i != null) ??
                                        context.RequestServices.GetRequiredService<IServiceInstanceChoose>();

            var feature = context.Features.GetOrAdd<ILoadBalanceFeature>(() => new LoadBalanceFeature());
            feature.ServiceInstanceChoose = serviceInstanceChoose;

            await _next(context);
        }

        private static IEnumerable<IServiceInstanceChoose> GetChoosers(IRabbitContext context)
        {
            return GetChooserNames(context).Where(i => !string.IsNullOrEmpty(i)).Select(name => context.RequestServices.GetRequiredNamedService<IServiceInstanceChoose>(name));
        }

        private static IEnumerable<string> GetChooserNames(IRabbitContext context)
        {
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