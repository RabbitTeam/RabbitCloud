using Microsoft.Extensions.Logging;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Config.Abstractions.Support;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.NetMQ.Internal;

namespace RabbitCloud.Rpc.NetMQ.Config
{
    public class NetMqProtocolProvider : IProtocolProvider
    {
        private readonly IFormatterFactory _formatterFactory;
        private readonly ILogger<RouterSocketFactory> _routerSocketFactoryLogger;

        public NetMqProtocolProvider(IFormatterFactory formatterFactory, ILogger<RouterSocketFactory> routerSocketFactoryLogger)
        {
            _formatterFactory = formatterFactory;
            _routerSocketFactoryLogger = routerSocketFactoryLogger;
        }

        #region Implementation of IProtocolProvider

        public string Name { get; } = "NetMQ";

        public IProtocol CreateProtocol(ProtocolConfig config)
        {
            var requestFormatter = _formatterFactory.GetRequestFormatter(config.Formatter);
            var responseFormatter = _formatterFactory.GetResponseFormatter(config.Formatter);

            var netMqPollerHolder = new NetMqPollerHolder();
            var routerSocketFactory = new RouterSocketFactory(netMqPollerHolder, _routerSocketFactoryLogger);

            return new NetMqProtocol(routerSocketFactory, requestFormatter, responseFormatter, netMqPollerHolder);
        }

        #endregion Implementation of IProtocolProvider
    }
}