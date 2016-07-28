using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers;
using Rabbit.Rpc.Runtime.Client.HealthChecks;
using Rabbit.Rpc.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Client.Implementation
{
    public class RemoteInvokeService : IRemoteInvokeService
    {
        private readonly IAddressResolver _addressResolver;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly ILogger<RemoteInvokeService> _logger;
        private readonly IHealthCheckService _healthCheckService;
        private readonly IServiceRouteManager _serviceRouteManager;

        public RemoteInvokeService(IAddressResolver addressResolver, ITransportClientFactory transportClientFactory, ILogger<RemoteInvokeService> logger, IHealthCheckService healthCheckService, IServiceRouteManager serviceRouteManager)
        {
            _addressResolver = addressResolver;
            _transportClientFactory = transportClientFactory;
            _logger = logger;
            _healthCheckService = healthCheckService;
            _serviceRouteManager = serviceRouteManager;
        }

        #region Implementation of IRemoteInvokeService

        public Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context)
        {
            return InvokeAsync(context, Task.Factory.CancellationToken);
        }

        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.InvokeMessage == null)
                throw new ArgumentNullException(nameof(context.InvokeMessage));

            if (string.IsNullOrEmpty(context.InvokeMessage.ServiceId))
                throw new ArgumentException("服务Id不能为空。", nameof(context.InvokeMessage.ServiceId));

            var serviceRoute = await _serviceRouteManager.GetAsync(context.InvokeMessage.ServiceId);
            var serviceDescriptor = serviceRoute.ServiceDescriptor;

            var invokeMessage = context.InvokeMessage;
            var address = await _addressResolver.Resolver(invokeMessage.ServiceId);

            if (address == null)
                throw new RpcException($"无法解析服务Id：{invokeMessage.ServiceId}的地址信息。");

            try
            {
                var client = _transportClientFactory.CreateClient(address.CreateEndPoint());
                var message = TransportMessage.CreateInvokeMessage(invokeMessage);

                //需要等待返回结果
                if (serviceDescriptor.WaitReturn())
                {
                    _logger.LogInformation($"服务 '{serviceDescriptor.Id}' 需要等待执行结果。");
                    var callbackTask = client.ReceiveAsync(message.Id);
                    await client.SendAsync(message);
                    var returnMessage = await callbackTask;
                    return returnMessage.GetContent<RemoteInvokeResultMessage>();
                }
                _logger.LogInformation($"服务 '{serviceDescriptor.Id}' 无需等待执行结果。");
                //无需等待返回结果
                await client.SendAsync(message);
                return null;
            }
            catch (RpcCommunicationException)
            {
                await _healthCheckService.MarkFailure(address);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError($"发起请求中发生了错误，服务Id：{invokeMessage.ServiceId}。", exception);
                throw;
            }
        }

        #endregion Implementation of IRemoteInvokeService
    }
}