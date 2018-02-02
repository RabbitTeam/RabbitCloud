using Rabbit.Go.Codec;
using Rabbit.Go.Formatters;
using Rabbit.Go.Interceptors;
using Rabbit.Go.Internal;
using System;
using System.Collections.Generic;
using System.Web;

namespace Rabbit.Go.Core.Internal
{
    public struct UrlDescriptor
    {
        public UrlDescriptor(string url)
        {
            var uri = new Uri(url, UriKind.Absolute);
            Scheme = uri.Scheme;
            Host = uri.Host;
            Port = uri.IsDefaultPort ? null : (int?)uri.Port;

            var pathAndQuery = HttpUtility.UrlDecode(uri.PathAndQuery);

            var queryStartIndex = pathAndQuery.IndexOf('?');

            if (queryStartIndex == -1)
            {
                Path = pathAndQuery;
                QueryString = "?";
            }
            else
            {
                Path = pathAndQuery.Substring(0, queryStartIndex);
                QueryString = pathAndQuery.Substring(queryStartIndex);
            }
        }

        public string Scheme { get; }
        public string Host { get; }
        public int? Port { get; }
        public string Path { get; }
        public string QueryString { get; }
    }

    public static class UrlDescriptorExtensions
    {
        public static bool HasQuery(this UrlDescriptor urlDescriptor)
        {
            return !string.IsNullOrEmpty(urlDescriptor.QueryString) && urlDescriptor.QueryString != "?";
        }
    }

    public class MethodInvokerEntry
    {
        public UrlDescriptor UrlTemplate { get; set; }
        public IGoClient Client { get; set; }
        public ICodec Codec { get; set; }
        public IKeyValueFormatterFactory KeyValueFormatterFactory { get; set; }
        public ITemplateParser TemplateParser { get; set; }
        public IList<IInterceptorMetadata> Interceptors { get; set; }
    }
}