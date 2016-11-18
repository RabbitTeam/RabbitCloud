using RabbitCloud.Abstractions;
using System;
using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions.Utils.Extensions
{
    public static class UrlExtensions
    {
        public static string GetProtocolKey(this Url url)
        {
            return $"{url.Scheme}://{url.Host}:{url.Port}{url.Path}";
        }

        public static string GetServiceKey(this Url url)
        {
            return ServiceKeyUtil.GetServiceKey(url.Path);
        }

        public static IDictionary<string, string> GetParameters(this Url url)
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var temp = url.Query.Trim('?').Split('&');
            foreach (var item in temp)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                var values = item.Split('=');
                parameters[values[0]] = values.Length == 1 ? string.Empty : values[1];
            }
            return parameters;
        }
    }
}