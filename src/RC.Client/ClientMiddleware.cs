using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
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

        public ClientMiddleware(RabbitRequestDelegate next, ILogger<ClientMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var serviceRequestFeature = context.Features.Get<IServiceRequestFeature>();
            try
            {
                await RequestAsync(context, serviceRequestFeature);
            }
            catch (Exception e)
            {
                var clientException = e as RabbitClientException ?? ExceptionUtilities.ServiceRequestFailure(serviceRequestFeature.ServiceName, 400, e);
                context.Response.StatusCode = clientException.StatusCode;

                throw clientException;
            }
        }

        private async Task RequestAsync(IRabbitContext context, IServiceRequestFeature serviceRequestFeature)
        {
            var requestOptions = serviceRequestFeature.RequestOptions;

            //最少调用一次
            var retries = Math.Max(requestOptions.MaxAutoRetries, 0) + 1;
            //最少使用一个服务
            var retriesNextServer = Math.Max(requestOptions.MaxAutoRetriesNextServer, 0) + 1;

            IList<Exception> exceptions = null;
            for (var i = 0; i < retriesNextServer; i++)
            {
                var getServiceInstance = serviceRequestFeature.GetServiceInstance;
                var serviceInstance = getServiceInstance();
                serviceRequestFeature.GetServiceInstance = () => serviceInstance;

                for (var j = 0; j < retries; j++)
                {
                    try
                    {
                        var codec = serviceRequestFeature.Codec;
                        if (codec != null)
                            context.Request.Body = codec.Encode(context.Request.Body);
                        await _next(context);
                        if (codec != null)
                            context.Response.Body = codec.Decode(context.Response.Body);
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
            if (exceptions != null && exceptions.Any())
                throw new AggregateException(exceptions);
        }
    }
}