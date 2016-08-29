using Cowboy.Sockets.Tcp.Server;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Default.Service.Message;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Default.Service
{
    public class ServerEntry : IDisposable
    {
        private readonly ICodec _codec;
        private readonly TcpSocketSaeaServer _server;

        public delegate Task<ResponseMessage> ReplyHandler(RequestMessage message);

        private ReplyHandler _replyHandler;

        public event ReplyHandler Received
        {
            add { _replyHandler += value; }
            remove { throw new NotImplementedException(); }
        }

        private async Task OnReceived(TcpSocketSaeaSession session, byte[] data)
        {
            if (_replyHandler == null)
                return;

            var requestMessage = _codec.DecodeByBytes<RequestMessage>(data);
            var response = await _replyHandler.Invoke(requestMessage);

            var sendData = _codec.EncodeToBytes(response);

            await session.SendAsync(sendData);
        }

        public ServerEntry(ICodec codec, TcpSocketSaeaServer server)
        {
            _codec = codec;
            _server = server;
        }

        public ServerEntry(ICodec codec, IPEndPoint ipEndPoint)
        {
            _codec = codec;
            _server = new TcpSocketSaeaServer(ipEndPoint, (session, data, offset, count) => OnReceived(session, data.Skip(offset).Take(count).ToArray()));
            _server.Listen();
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            _server.Dispose();
        }

        #endregion Implementation of IDisposable
    }

    public interface IServerTable : IDisposable
    {
        ServerEntry OpenServer(EndPoint endPoint, IExporter exporter);
    }

    public class ServerTable : IServerTable
    {
        private readonly ICodec _codec;

        private readonly ConcurrentDictionary<string, ServerEntry> _serverEntries =
            new ConcurrentDictionary<string, ServerEntry>();

        public ServerTable(ICodec codec)
        {
            _codec = codec;
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            foreach (var entry in _serverEntries.Values)
            {
                try
                {
                    entry.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            _serverEntries.Clear();
        }

        public ServerEntry OpenServer(EndPoint endPoint, IExporter exporter)
        {
            var ipEndPoint = (IPEndPoint)endPoint;
            var serverKey = ipEndPoint.ToString();
            return _serverEntries.GetOrAdd(serverKey, k =>
            {
                var server = new ServerEntry(_codec, ipEndPoint);

                server.Received += async request =>
                {
                    var invocation = request.Invocation;

                    var result = await exporter.Invoker.Invoke(invocation);
                    return ResponseMessage.Create(request, result.Value, result.Exception);
                };

                return server;
            });
        }

        #endregion Implementation of IDisposable
    }
}