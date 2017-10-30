using Grpc.Core;
using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Grpc.Client
{
    internal class ChannelServiceInstance : IEqualityComparer<IServiceInstance>
    {
        public static ChannelServiceInstance Instance = new ChannelServiceInstance();

        #region Implementation of IEqualityComparer<in IServiceInstance>

        /// <inheritdoc />
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(IServiceInstance x, IServiceInstance y)
        {
            return string.Equals(x.Host, y.Host, StringComparison.OrdinalIgnoreCase) && x.Port == y.Port;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj">obj</paramref> is a reference type and <paramref name="obj">obj</paramref> is null.</exception>
        public int GetHashCode(IServiceInstance obj)
        {
            unchecked
            {
                return ((obj.Host != null ? obj.Host.GetHashCode() : 0) * 397) ^ obj.Port.GetHashCode();
            }
        }

        #endregion Implementation of IEqualityComparer<in IServiceInstance>
    }

    public class ChannelTable
    {
        private readonly ConcurrentDictionary<IServiceInstance, Lazy<Channel>> _channels = new ConcurrentDictionary<IServiceInstance, Lazy<Channel>>(ChannelServiceInstance.Instance);

        public Channel GetChannel(IServiceInstance serviceInstance)
        {
            return _channels.GetOrAdd(serviceInstance, instance => new Lazy<Channel>(() => new Channel(instance.Host, instance.Port, ChannelCredentials.Insecure))).Value;
        }
    }

    public class DefaultChannelLocator : IChannelLocator
    {
        private readonly IDiscoveryClient _discoveryClient;
        private readonly ChannelTable _channelTable;

        public DefaultChannelLocator(IDiscoveryClient discoveryClient, ChannelTable channelTable)
        {
            _discoveryClient = discoveryClient;
            _channelTable = channelTable;
        }

        #region Implementation of IChannelLocator

        public Channel Locate(string serviceId)
        {
            var instances = _discoveryClient.GetInstances(serviceId);
            var index = new Random().Next(0, instances.Count);
            var channel = _channelTable.GetChannel(instances.ElementAt(index));
            return channel;
        }

        #endregion Implementation of IChannelLocator
    }
}