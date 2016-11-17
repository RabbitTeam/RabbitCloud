using RabbitCloud.Rpc.Abstractions.Protocol;

namespace RabbitCloud.Config.Abstractions.Builder
{
    public class ProtocolBuilder : Builder
    {
        private IProtocol _protocol;

        public ProtocolBuilder UseProtocol(IProtocol protocol)
        {
            _protocol = protocol;
            return this;
        }

        public IProtocol Build()
        {
            return _protocol;
        }
    }
}