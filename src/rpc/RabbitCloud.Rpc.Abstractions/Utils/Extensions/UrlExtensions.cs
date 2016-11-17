using RabbitCloud.Abstractions;

namespace RabbitCloud.Rpc.Abstractions.Utils.Extensions
{
    public static class UrlExtensions
    {
        public static string GetProtocolKey(this Url url)
        {
            return $"{url.Scheme}://{url.Host}:{url.Port}{url.AbsolutePath}";
        }

        public static string GetServiceKey(this Url url)
        {
            return ServiceKeyUtil.GetServiceKey(url.AbsolutePath);
        }
    }
}