using System;
using System.Collections.Generic;
using System.Text;
using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Registry.Consul;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.Formatters.Json;
using RabbitCloud.Rpc.NetMQ;

namespace ConsoleApp
{
    public class ProtocolFactory
    {
        private readonly IServiceProvider _container;

        public ProtocolFactory(IServiceProvider container)
        {
            _container = container;
        }

        public IProtocol GetProtocol(string name)
        {
            switch (name)
            {
                case "netmq":
                    return _container.GetRequiredService<NetMqProtocol>();
            }
            throw new NotSupportedException(name);
        }
    }

    public class FormatterFactory
    {
        private readonly IServiceProvider _container;

        public FormatterFactory(IServiceProvider container)
        {
            _container = container;
        }

        public IRequestFormatter GetRequestFormatter(string name)
        {
            switch (name)
            {
                case "json":
                    return _container.GetRequiredService<JsonRequestFormatter>();
            }
            throw new NotSupportedException(name);
        }

        public IResponseFormatter GetResponseFormatter(string name)
        {
            switch (name)
            {
                case "json":
                    return _container.GetRequiredService<JsonResponseFormatter>();
            }
            throw new NotSupportedException(name);
        }
    }

    public class RegistryTableFactory
    {
        private readonly IServiceProvider _container;

        public RegistryTableFactory(IServiceProvider container)
        {
            _container = container;
        }

        public IRegistryTable GetRegistryTable(string protocol, IDictionary<string, string> parameters)
        {
            var consulClient = new ConsulClient(options =>
            {
                options.Address = new Uri(parameters["Address"]);
            });
            return new ConsulRegistryTable(consulClient, new HeartbeatManager(consulClient), _container.GetRequiredService<ILogger<ConsulRegistryTable>>());
        }
    }
}
