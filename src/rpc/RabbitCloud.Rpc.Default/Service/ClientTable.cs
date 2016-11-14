using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions.Codec;
using System;
using System.Collections.Concurrent;

namespace RabbitCloud.Rpc.Default.Service
{
    public interface IClientTable
    {
        CowboyClient OpenClient(Url url);
    }

    public class ClientTable : IClientTable
    {
        private readonly ICodec _codec;
        private readonly ConcurrentDictionary<string, Lazy<CowboyClient>> _clientEntries = new ConcurrentDictionary<string, Lazy<CowboyClient>>();

        public ClientTable(ICodec codec)
        {
            _codec = codec;
        }

        #region Implementation of IClientTable

        public CowboyClient OpenClient(Url url)
        {
            var key = $"{url.Host}:{url.Port}".ToLower();

            return _clientEntries.GetOrAdd(key, k => new Lazy<CowboyClient>(() => new CowboyClient(url, _codec))).Value;
        }

        #endregion Implementation of IClientTable
    }
}