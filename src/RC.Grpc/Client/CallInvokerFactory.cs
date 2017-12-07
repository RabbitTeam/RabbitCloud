using Grpc.Core;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Client;
using Rabbit.Cloud.Grpc.Client.Internal;
using System;
using System.Collections.Concurrent;

namespace Rabbit.Cloud.Grpc.Client
{
    public class CallInvokerFactory : ICallInvokerFactory
    {
        private readonly ConcurrentDictionary<IServiceInstance, Lazy<CallInvoker>> _callInvokers = new ConcurrentDictionary<IServiceInstance, Lazy<CallInvoker>>(ChannelPool.ChannelServiceInstanceComparer.Instance);

        private readonly ChannelPool _channelPool;

        public CallInvokerFactory(ChannelPool channelPool)
        {
            _channelPool = channelPool;
        }

        public CallInvoker GetCallInvoker(string host, int port)
        {
            return _callInvokers.GetOrAdd(new ChannelPool.ServiceInstance(host, port), key => new Lazy<CallInvoker>(() => new DefaultCallInvoker(_channelPool.GetChannel(key)))).Value;
        }
    }
}