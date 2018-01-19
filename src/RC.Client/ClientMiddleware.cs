using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class ClientMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ILogger<ClientMiddleware> _logger;
        private readonly RabbitClientOptions _options;

        public ClientMiddleware(RabbitRequestDelegate next, ILogger<ClientMiddleware> logger, IOptions<RabbitClientOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var rabbitClientFeature = context.Features.Get<IRabbitClientFeature>();
            try
            {
                await RequestAsync(context, rabbitClientFeature);
            }
            catch (Exception e)
            {
                var clientException = e as RabbitClientException ?? ExceptionUtilities.ServiceRequestFailure(context.Request.Host, 400, e);
                context.Response.StatusCode = clientException.StatusCode;

                throw clientException;
            }
        }

        private ILoadBalanceFeature EnsureLoadBalanceFeature(IRabbitContext context, ServiceRequestOptions requestOptions)
        {
            var loadBalanceFeature = context.Features.Get<ILoadBalanceFeature>();
            if (loadBalanceFeature == null)
                context.Features.Set(loadBalanceFeature = new LoadBalanceFeature());

            loadBalanceFeature.ServiceInstanceChooser =
                _options.Choosers.Get(requestOptions.ServiceChooser) ?? _options.DefaultChooser;

            return loadBalanceFeature;
        }

        private async Task RequestAsync(IRabbitContext context, IRabbitClientFeature rabbitClientFeature)
        {
            var requestOptions = rabbitClientFeature?.RequestOptions ?? _options.DefaultRequestOptions;

            //最少调用一次
            var retries = Math.Max(requestOptions.MaxAutoRetries, 0) + 1;

            IList<Exception> exceptions = new List<Exception>();

            var serviceDiscoveryFeature = context.Features.Get<IServiceDiscoveryFeature>();
            var serviceInstances = serviceDiscoveryFeature?.ServiceInstances;

            if (serviceInstances == null || serviceInstances.Count <= 1)
            {
                await InvokeServiceInstanceAsync(context, retries, exceptions);
            }
            else
            {
                void SetAvailableServiceInstances(IEnumerable<IServiceInstance> ignores = null)
                {
                    // 没有启用服务发现，则直接调用
                    if (serviceInstances == null)
                        return;

                    // 没有忽略任何服务实例（第一次调用），则返回全部实例
                    if (ignores == null)
                        return;

                    // 如果全部的实例都被忽略，则初始化状态 从第一个继续开始
                    serviceInstances = serviceInstances.Any() ? serviceInstances.Except(ignores).ToArray() : new List<IServiceInstance>(ignores).ToArray();

                    serviceDiscoveryFeature.ServiceInstances = serviceInstances;
                }

                var loadBalanceFeature = EnsureLoadBalanceFeature(context, requestOptions);

                //允许切换服务实例的次数
                var retriesNextServer = Math.Max(requestOptions.MaxAutoRetriesNextServer, 0) + 1;

                IList<IServiceInstance> errorServiceInstances = null;
                for (var i = 0; i < retriesNextServer; i++)
                {
                    await InvokeServiceInstanceAsync(context, retries, exceptions);

                    var requestInstance = loadBalanceFeature.RequestInstance;
                    if (requestInstance == null)
                        continue;

                    if (errorServiceInstances == null)
                        errorServiceInstances = new List<IServiceInstance>();

                    errorServiceInstances.Add(requestInstance);

                    // 忽略已经调用过的服务实例
                    SetAvailableServiceInstances(errorServiceInstances);
                }
            }

            if (exceptions != null && exceptions.Any())
                throw new AggregateException(exceptions);
        }

        private async Task InvokeServiceInstanceAsync(IRabbitContext context, int retries, IList<Exception> exceptions)
        {
            for (var j = 0; j < retries; j++)
            {
                try
                {
                    await _next(context);
                    return;
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>();

                    exceptions.Add(e);

                    _logger.LogError(e, "请求失败。");

                    //只有服务器错误才进行重试
                    if (!(e is RabbitClientException rabbitClientException) ||
                        rabbitClientException.StatusCode < 500)
                        throw;
                }
            }
        }
    }
}