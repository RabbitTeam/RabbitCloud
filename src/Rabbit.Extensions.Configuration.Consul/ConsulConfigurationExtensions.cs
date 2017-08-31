using Consul;
using Microsoft.Extensions.Configuration;
using System;

namespace Rabbit.Extensions.Configuration.Consul
{
    public static class ConsulConfigurationExtensions
    {
        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder builder, string url, string path = "/", string datacenter = null)
        {
            return builder.AddConsul(new Uri(url), path, datacenter);
        }

        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder builder, Uri address, string path = "/", string datacenter = null)
        {
            return builder.AddConsul(c =>
            {
                c.Address = address;
                c.Datacenter = datacenter;
            }, s =>
            {
                s.Path = path;
            });
        }

        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder builder, ConsulClient consulClient, string path = "/")
        {
            return builder.AddConsul(s =>
            {
                s.ConsulClient = consulClient;
            });
        }

        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder builder, Action<ConsulClientConfiguration> clientConfigure, Action<ConsulConfigurationSource> configureSource)
        {
            return builder.AddConsul(s =>
            {
                s.ConsulClient = new ConsulClient(clientConfigure);
                configureSource?.Invoke(s);
            });
        }

        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder builder,
            Action<ConsulConfigurationSource> configureSource) => builder.Add(configureSource);
    }
}