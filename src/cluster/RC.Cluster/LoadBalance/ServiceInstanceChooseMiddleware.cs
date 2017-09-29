using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Abstractions.Features;
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

        public async Task Invoke(RabbitContext context)
        {
            var serviceInstanceChoose = GetChoosers(context).FirstOrDefault(i => i != null) ??
                                        context.RequestServices.GetRequiredService<IServiceInstanceChoose>();

            var feature = context.Features.GetOrAdd<ILoadBalanceFeature>(() => new LoadBalanceFeature());
            feature.ServiceInstanceChoose = serviceInstanceChoose;
            Console.WriteLine(serviceInstanceChoose);

            await _next(context);
        }

        private static IEnumerable<IServiceInstanceChoose> GetChoosers(RabbitContext context)
        {
            return GetChooserNames(context).Where(i => !string.IsNullOrEmpty(i)).Select(name => context.RequestServices.GetRequiredNamedService<IServiceInstanceChoose>(name));
        }

        private static IEnumerable<string> GetChooserNames(RabbitContext context)
        {
            var message = context.Request.RequestMessage;
            var querys = QueryHelpers.ParseNullableQuery(message.RequestUri.Query);

            const string key = LoadBalanceConstants.ChooserKey;

            IEnumerable<string> items = querys?.FirstOrDefault(i => key.Equals(i.Key, StringComparison.OrdinalIgnoreCase)).Value;
            if (items != null)
                foreach (var value in items)
                    yield return value;

            items = message.Headers.FirstOrDefault(i => key.Equals(i.Key, StringComparison.OrdinalIgnoreCase)).Value;
            if (items == null) yield break;
            foreach (var value in items)
                yield return value;
        }
    }
}