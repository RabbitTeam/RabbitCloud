using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Proxy;

namespace RabbitCloud.Config.Abstractions.Builder
{
    public class ApplicationBuilder
    {
        private readonly ApplicationConfigModel _model = new ApplicationConfigModel();

        public ApplicationBuilder UseRegistry(IRegistry registry)
        {
            _model.Registry = registry;
            return this;
        }

        public ApplicationBuilder UseProtocol(IProtocol protocol)
        {
            _model.Protocol = protocol;
            return this;
        }

        public ApplicationBuilder UseContainers(ContainerConfigModel[] containers)
        {
            _model.Containers = containers;
            return this;
        }

        public ApplicationBuilder UseProxyFactory(IProxyFactory proxyFactory)
        {
            _model.ProxyFactory = proxyFactory;
            return this;
        }

        public ApplicationConfigModel Build()
        {
            return _model;
        }
    }
}