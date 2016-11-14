using RabbitCloud.Abstractions;

namespace RabbitCloud.Rpc.Abstractions.Utils.Extensions
{
    public static class UrlExtensions
    {
        public static string GetProtocolKey(this Url url)
        {
            return $"{url.Scheme}://{url.Host}:{url.Port}{url.AbsolutePath}";
        }
    }
}