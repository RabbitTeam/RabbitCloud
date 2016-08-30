using Cowboy.Sockets.Tcp;
using Cowboy.Sockets.Tcp.Client;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Default.Service.Message;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Id = RabbitCloud.Rpc.Default.Service.Message.Id;

namespace RabbitCloud.Rpc.Default.Service
{
    public class ClientEntry : IDisposable
    {
        private readonly IPEndPoint _ipEndPoint;
        private readonly ICodec _codec;
        private TcpSocketSaeaClient _client;
        private readonly ConcurrentDictionary<Id, TaskCompletionSource<ResponseMessage>> _callback;
        private readonly Action _dispose;

        private TcpSocketSaeaClient Client
        {
            get
            {
                if (_client == null || _client.State == TcpSocketConnectionState.Closed)
                    _client = CreateClient();
                return _client;
            }
        }

        public ClientEntry(IPEndPoint ipEndPoint, ICodec codec,
            ConcurrentDictionary<Id, TaskCompletionSource<ResponseMessage>> callback, Action dispose)
        {
            _ipEndPoint = ipEndPoint;
            _codec = codec;
            _callback = callback;
            _dispose = dispose;
        }

        public async Task<ResponseMessage> Send(RequestMessage requestMessage)
        {
            var source = new TaskCompletionSource<ResponseMessage>();
            _callback.TryAdd(requestMessage.Id, source);

            try
            {
                var data = _codec.EncodeToBytes(requestMessage);

                await Client.SendAsync(data);
            }
            catch
            {
                TaskCompletionSource<ResponseMessage> value;
                _callback.TryRemove(requestMessage.Id, out value);
            }

            return await source.Task;
        }

        private TcpSocketSaeaClient CreateClient()
        {
            var client = new TcpSocketSaeaClient(_ipEndPoint, (c, data, offset, count) =>
            {
                var responseMessage = _codec.DecodeByBytes<ResponseMessage>(data.Skip(offset).Take(count).ToArray());
                TaskCompletionSource<ResponseMessage> source;
                _callback.TryRemove(responseMessage.Id, out source);
                source.TrySetResult(responseMessage);

                return Task.CompletedTask;
            });
            client.Connect().Wait();
            return client;
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _client.Close();
            });
            _dispose?.Invoke();
        }

        #endregion Implementation of IDisposable
    }

    public interface IClientTable
    {
        ClientEntry OpenClient(EndPoint endPoint);
    }

    public class ClientTable : IClientTable
    {
        private readonly ICodec _codec;
        private readonly ConcurrentDictionary<Id, TaskCompletionSource<ResponseMessage>> _callbacks = new ConcurrentDictionary<Id, TaskCompletionSource<ResponseMessage>>();
        private readonly ConcurrentDictionary<string, ClientEntry> _clientEntries = new ConcurrentDictionary<string, ClientEntry>();

        public ClientTable(ICodec codec)
        {
            _codec = codec;
        }

        #region Implementation of IClientTable

        public ClientEntry OpenClient(EndPoint endPoint)
        {
            var ipEndPoint = (IPEndPoint)endPoint;
            var key = ipEndPoint.ToString();
            return _clientEntries.GetOrAdd(key, k =>
            {
                return new ClientEntry(ipEndPoint, _codec, _callbacks, () =>
                {
                    ClientEntry value;
                    _clientEntries.TryRemove(k, out value);
                });
            });
        }

        #endregion Implementation of IClientTable
    }
}