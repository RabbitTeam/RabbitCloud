using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class ClientMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ILogger<ClientMiddleware> _logger;

        public ClientMiddleware(RabbitRequestDelegate next, ILogger<ClientMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(IRabbitContext context)
        {
            await RequestAsync(context);
        }

        private async Task RequestAsync(IRabbitContext context)
        {
            var serviceRequestFeature = context.Features.Get<IServiceRequestFeature>();

            var requestOptions = serviceRequestFeature.RequestOptions;

            //最少调用一次
            var retries = Math.Max(requestOptions.MaxAutoRetries, 0) + 1;
            //最少使用一个服务
            var retriesNextServer = Math.Max(requestOptions.MaxAutoRetriesNextServer, 0) + 1;

            var serviceInstances = serviceRequestFeature.ServiceInstances;

            if (serviceInstances == null || !serviceInstances.Any())
                throw ExceptionUtilities.NotFindServiceInstance(serviceRequestFeature.ServiceName);

            var chooser = serviceRequestFeature.Chooser;
            IList<IServiceInstance> invokedServiceInstances = null;

            void AddInvoked(IServiceInstance serviceInstance)
            {
                if (invokedServiceInstances == null)
                    invokedServiceInstances = new List<IServiceInstance>();

                invokedServiceInstances.Add(serviceInstance);
            }

            IReadOnlyList<IServiceInstance> GetAvailableServiceInstances()
            {
                //没有任何调用过的服务实例则全部返回，否则过滤掉已经调用过的服务实例
                if (invokedServiceInstances == null || !invokedServiceInstances.Any())
                    return serviceInstances;

                //所有的服务实例都已经被调用过，则清除重新开始
                if (invokedServiceInstances.Count == serviceInstances.Count)
                    invokedServiceInstances.Clear();

                return serviceInstances.Except(invokedServiceInstances).ToArray();
            }

            IServiceInstance ChooseServiceInstance()
            {
                return chooser.Choose(GetAvailableServiceInstances());
            }

            Exception lastException = null;
            for (var i = 0; i < retriesNextServer; i++)
            {
                var serviceInstance = ChooseServiceInstance();
                serviceRequestFeature.ServiceInstance = serviceInstance;

                for (var j = 0; j < retries; j++)
                {
                    try
                    {
                        await _next(context);
                        return;
                    }
                    catch (Exception e)
                    {
                        lastException = e;
                        _logger.LogError(e, "请求失败。");

                        //只有服务器错误才进行重试
                        if (!(e is RabbitClientException rabbitClientException) ||
                            rabbitClientException.StatusCode < 500)
                            throw;

                        AddInvoked(serviceInstance);
                    }
                }
            }

            if (lastException != null)
                throw lastException;
        }
    }
}