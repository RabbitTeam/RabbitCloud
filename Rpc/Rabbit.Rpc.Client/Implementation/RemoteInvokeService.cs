using Rabbit.Rpc.Client.Address.Resolvers;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Client.Implementation
{
    public class RemoteInvokeService : IRemoteInvokeService
    {
        private readonly IAddressResolver _addressResolver;
        private readonly ITransportClientFactory _transportClientFactory;

        public RemoteInvokeService(IAddressResolver addressResolver, ITransportClientFactory transportClientFactory, ISerializer serializer)
        {
            _addressResolver = addressResolver;
            _transportClientFactory = transportClientFactory;
        }

        #region Implementation of IRemoteInvokeService

        public Task<TransportMessage> InvokeAsync(RemoteInvokeContext context)
        {
            return InvokeAsync(context, Task.Factory.CancellationToken);
        }

        public async Task<TransportMessage> InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.InvokeMessage == null)
                throw new ArgumentNullException(nameof(context.InvokeMessage));

            if (string.IsNullOrEmpty(context.InvokeMessage.ServiceId))
                throw new ArgumentException("服务Id不能为空。", nameof(context.InvokeMessage.ServiceId));

            var invokeMessage = context.InvokeMessage;
            var address = await _addressResolver.Resolver(invokeMessage.ServiceId);

            if (address == null)
                throw new RpcException($"无法解析服务Id：{invokeMessage.ServiceId}的地址信息。");

            var client = _transportClientFactory.CreateClient(address.CreateEndPoint());
            var message = new TransportMessage<RemoteInvokeMessage>
            {
                Content = context.InvokeMessage,
                Id = Guid.NewGuid().ToString("N")
            };
            await client.SendAsync(TransportMessage.Convert(message));
            var resultMessage = await client.ReceiveAsync(message.Id);
            return resultMessage;
        }

        #endregion Implementation of IRemoteInvokeService
    }
}