using Cowboy.Sockets.Tcp.Client;
using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Codec;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Default.Service
{
    public class CowboyClient : IDisposable
    {
        private readonly ConcurrentDictionary<long, TaskCompletionSource<IResponse>> _callback = new ConcurrentDictionary<long, TaskCompletionSource<IResponse>>();
        private readonly ICodec _codec;
        protected Url Url { get; set; }
        private TcpSocketSaeaClient _client;

        public CowboyClient(Url url, ICodec codec)
        {
            _codec = codec;
            Url = url;

            Task.Run(DoOpen).Wait();
        }

        public async Task<IResponse> Send(IRequest requestMessage)
        {
            var source = new TaskCompletionSource<IResponse>();
            _callback.TryAdd(requestMessage.RequestId, source);

            try
            {
                var data = (byte[])_codec.Encode(requestMessage);

                await _client.SendAsync(data);
            }
            catch
            {
                TaskCompletionSource<IResponse> value;
                _callback.TryRemove(requestMessage.RequestId, out value);
            }

            return await source.Task;
        }

        private async Task DoOpen()
        {
            var address = await Dns.GetHostAddressesAsync(Url.Host);
            var ipEndPoint = new IPEndPoint(address.First(), Url.Port);
            _client = new TcpSocketSaeaClient(ipEndPoint, (c, data, offset, count) =>
             {
                 var responseMessage = (IResponse)_codec.Decode(data.Skip(offset).Take(count).ToArray(), typeof(IResponse));
                 TaskCompletionSource<IResponse> source;
                 _callback.TryRemove(responseMessage.RequestId, out source);
                 source.TrySetResult(responseMessage);

                 return Task.CompletedTask;
             });
            await _client.Connect();
        }

        private async Task DoClose()
        {
            if (_client == null)
                return;
            await _client.Close();
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            DoClose().Wait();
        }

        #endregion Implementation of IDisposable
    }
}