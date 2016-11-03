using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
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
        private readonly ConcurrentDictionary<string, CowboyClient> _clientEntries = new ConcurrentDictionary<string, CowboyClient>();

        public ClientTable(ICodec codec)
        {
            _codec = codec;
        }

        #region Implementation of IClientTable

        public CowboyClient OpenClient(Url url)
        {
            var key = $"{url.Host}:{url.Port}".ToLower();
            return _clientEntries.GetOrAdd(key, k => new CowboyClient(url, _codec));
        }

        #endregion Implementation of IClientTable
    }
}