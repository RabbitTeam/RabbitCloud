using Grpc.Core;
using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Client.Internal
{
    public class ChannelPool
    {
        private readonly ConcurrentDictionary<IServiceInstance, Lazy<Channel>> _channels = new ConcurrentDictionary<IServiceInstance, Lazy<Channel>>(ChannelServiceInstanceComparer.Instance);

        public Channel GetChannel(IServiceInstance serviceInstance)
        {
            return _channels.GetOrAdd(serviceInstance, instance => new Lazy<Channel>(() => new Channel(instance.Host, instance.Port, ChannelCredentials.Insecure))).Value;
        }

        public Channel GetChannel(string host, int port)
        {
            return _channels.GetOrAdd(new ServiceInstance(host, port), instance => new Lazy<Channel>(() => new Channel(instance.Host, instance.Port, ChannelCredentials.Insecure))).Value;
        }

        internal class ChannelServiceInstanceComparer : IEqualityComparer<IServiceInstance>
        {
            public static ChannelServiceInstanceComparer Instance = new ChannelServiceInstanceComparer();

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

        internal struct ServiceInstance : IServiceInstance
        {
            public ServiceInstance(string host, int port)
            {
                ServiceId = null;
                Metadata = null;
                Host = host.ToLower();
                Port = port;
            }

            #region Implementation of IServiceInstance

            public string ServiceId { get; }
            public string Host { get; }
            public int Port { get; }
            public IDictionary<string, string> Metadata { get; }

            #endregion Implementation of IServiceInstance
        }
    }
}