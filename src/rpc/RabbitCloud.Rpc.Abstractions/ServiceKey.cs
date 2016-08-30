/*using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;

namespace RabbitCloud.Rpc.Abstractions
{
    public struct ServiceKey
    {
        public string Key { get; set; }

        public ServiceKey(string key)
        {
            Key = key;
        }

        public static implicit operator ServiceKey(Url url)
        {
            return new ServiceKey(url.GetServiceKey());
        }
    }
}*/