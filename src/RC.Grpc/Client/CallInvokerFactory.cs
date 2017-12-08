using Grpc.Core;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Client;
using Rabbit.Cloud.Grpc.Client.Internal;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Grpc.Client
{
    public class CallInvokerFactory : ICallInvokerFactory
    {
        private readonly ConcurrentDictionary<IServiceInstance, CallInvoker> _callInvokers = new ConcurrentDictionary<IServiceInstance, CallInvoker>(ChannelPool.ChannelServiceInstanceComparer.Instance);

        private readonly ChannelPool _channelPool;

        public CallInvokerFactory(ChannelPool channelPool)
        {
            _channelPool = channelPool;
        }

        public async Task<CallInvoker> GetCallInvokerAsync(string host, int port, TimeSpan timeout)
        {
            var key = new ChannelPool.ServiceInstance(host, port);
            var channel = _channelPool.GetChannel(key);

            await channel.ConnectAsync(DateTime.UtcNow.Add(timeout));

            return _callInvokers.GetOrAdd(key, new DefaultCallInvoker(channel));
        }
    }
}