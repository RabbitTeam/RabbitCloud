using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Config.Abstractions.Support;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.NetMQ.Internal;
using System;

namespace RabbitCloud.Rpc.NetMQ.Config
{
    public class NetMqProtocolProvider : IProtocolProvider
    {
        private readonly IServiceProvider _container;
        private readonly IFormatterFactory _formatterFactory;

        public NetMqProtocolProvider(IServiceProvider container, IFormatterFactory formatterFactory)
        {
            _container = container;
            _formatterFactory = formatterFactory;
        }

        #region Implementation of IProtocolProvider

        public string Name { get; } = "NetMQ";

        public IProtocol CreateProtocol(ProtocolConfig config)
        {
            var requestFormatter = _formatterFactory.GetRequestFormatter(config.Formatter);
            var responseFormatter = _formatterFactory.GetResponseFormatter(config.Formatter);
            var netMqPollerHolder = new NetMqPollerHolder();
            return new NetMqProtocol(new RouterSocketFactory(netMqPollerHolder, _container.GetRequiredService<ILogger<RouterSocketFactory>>()), requestFormatter, responseFormatter, netMqPollerHolder);
        }

        #endregion Implementation of IProtocolProvider
    }
}