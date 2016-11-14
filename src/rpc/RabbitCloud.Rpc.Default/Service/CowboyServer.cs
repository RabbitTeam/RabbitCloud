using Cowboy.Sockets.Tcp.Server;
using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Codec;
using RabbitCloud.Rpc.Abstractions.Internal;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Default.Service
{
    public class CowboyServer : IDisposable
    {
        private readonly Func<IRequest, Task<IResponse>> _onReceived;
        private readonly ICodec _codec;
        private TcpSocketSaeaServer _server;
        protected Url Url { get; set; }

        public CowboyServer(Url url, ICodec codec, Func<IRequest, Task<IResponse>> onReceived)
        {
            Url = url;
            _codec = codec;
            _onReceived = onReceived;

            Task.Run(DoOpen).Wait();
        }

        private async Task DoOpen()
        {
            var address = await Dns.GetHostAddressesAsync(Url.Host);

            var ipEndPoint = new IPEndPoint(address.First(), Url.Port);
            _server = new TcpSocketSaeaServer(ipEndPoint,
                async (session, data, offset, count) =>
                {
                    var buffer = data.Skip(offset).Take(count).ToArray();

                    var requestMessage = (IRequest)_codec.Decode(buffer, typeof(IRequest));
                    var response = (DefaultResponse)(await _onReceived(requestMessage));
                    response.RequestId = requestMessage.RequestId;

                    var sendData = (byte[])_codec.Encode(response);

                    await session.SendAsync(sendData);
                });
            _server.Listen();
        }

        private void DoClose()
        {
            _server?.Dispose();
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            DoClose();
        }

        #endregion Implementation of IDisposable
    }
}