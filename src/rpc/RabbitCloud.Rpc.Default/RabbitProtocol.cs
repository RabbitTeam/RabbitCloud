using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Default.Service;
using System.Net;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitProtocol : IProtocol
    {
        private readonly IServerTable _serverTable;
        private readonly IClientTable _clientTable;

        public RabbitProtocol(IServerTable serverTable, IClientTable clientTable)
        {
            _serverTable = serverTable;
            _clientTable = clientTable;
        }

        #region Implementation of IProtocol

        public IExporter Export(IInvoker invoker)
        {
            var exporter = new RabbitExporter(invoker);
            _serverTable.OpenServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9981), exporter);
            return exporter;
        }

        public IInvoker Refer(Id id)
        {
            var client = _clientTable.OpenClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9981));
            return new RabbitInvoker(id, client);
        }

        #endregion Implementation of IProtocol
    }
}