using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Rabbit.Go
{
    public class RequestContext
    {
        public RequestContext(string url) : this()
        {
            var uri = new Uri(url);
            Scheme = uri.Scheme;
            Host = uri.Host;
            Port = uri.Port;

            var pathAndQuery = uri.PathAndQuery;

            var queryStartIndex = pathAndQuery.IndexOf('?');
            if (queryStartIndex == -1)
            {
                Path = pathAndQuery;
            }
            else
            {
                Path = pathAndQuery.Substring(0, queryStartIndex);
                var querys = HttpUtility.ParseQueryString(pathAndQuery.Substring(queryStartIndex));
                foreach (var key in querys.AllKeys)
                    Query[key] = querys[key];
            }
        }

        public RequestContext(string url, IDictionary<string, StringValues> headers) : this(url)
        {
            Headers = new Dictionary<string, StringValues>(headers, StringComparer.OrdinalIgnoreCase);
        }

        public RequestContext()
        {
            Query = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            Headers = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        public string Method { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        private string _path;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;

                if (string.IsNullOrEmpty(_path))
                {
                    _path = "/";
                    return;
                }

                if (!_path.StartsWith("/"))
                    _path = "/" + _path;
            }
        }

        public IDictionary<string, StringValues> Query { get; internal set; }
        public IDictionary<string, StringValues> Headers { get; internal set; }
        public string Charset { get; set; }
        public byte[] Body { get; set; }
    }

    public static class RequestContextExtensions
    {
        public static RequestContext AppendQuery(this RequestContext context, string name, StringValues values)
        {
            Append(context.Query, name, values);
            return context;
        }

        public static RequestContext AppendHeader(this RequestContext context, string name, StringValues values)
        {
            Append(context.Headers, name, values);
            return context;
        }

        public static RequestContext SetBody(this RequestContext context, byte[] body)
        {
            context.Body = body;

            return context;
        }

        public static RequestContext SetBody(this RequestContext context, string bodyContent)
        {
            context.Body = bodyContent == null ? null : Encoding.UTF8.GetBytes(bodyContent);

            return context;
        }

        public static RequestContext SetCharset(this RequestContext context, byte[] body)
        {
            context.Body = body;

            return context;
        }

        public static RequestContext SetMethod(this RequestContext context, string method)
        {
            context.Method = method;

            return context;
        }

        private static void Append(IDictionary<string, StringValues> source, string name, StringValues values)
        {
            if (!source.TryGetValue(name, out var current))
                source[name] = values;

            source[name] = StringValues.Concat(current, values);
        }
    }
}