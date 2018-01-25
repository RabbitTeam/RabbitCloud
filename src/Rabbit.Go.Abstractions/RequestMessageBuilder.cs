using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Rabbit.Go
{
    public class UrlDescriptor
    {
        public UrlDescriptor(UrlDescriptor urlDescriptor)
        {
            Scheme = urlDescriptor.Scheme;
            Host = urlDescriptor.Host;
            Port = urlDescriptor.Port;
            Path = urlDescriptor.Path;
            Query = new Dictionary<string, StringValues>(urlDescriptor.Query, StringComparer.OrdinalIgnoreCase);
        }

        public UrlDescriptor(string url) : this()
        {
            var uri = new Uri(url);
            Scheme = uri.Scheme;
            Host = uri.Host;
            Port = uri.IsDefaultPort ? null : (int?)uri.Port;

            var pathAndQuery = HttpUtility.UrlDecode(uri.PathAndQuery);

            var queryStartIndex = pathAndQuery.IndexOf('?');
            if (queryStartIndex == -1)
            {
                Path = pathAndQuery;
            }
            else
            {
                Path = pathAndQuery.Substring(0, queryStartIndex);
                var querys = QueryHelpers.ParseNullableQuery(pathAndQuery.Substring(queryStartIndex));
                foreach (var item in querys)
                    Query[item.Key] = querys[item.Value];
            }
        }

        public UrlDescriptor()
        {
            Query = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        public string Scheme { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
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

        public IDictionary<string, StringValues> Query { get; }

        #region Overrides of Object

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var uri = Port.HasValue ? $"{Scheme}://{Host}:{Port}{Path}" : $"{Scheme}://{Host}{Path}";
            if (Query.Any())
            {
                uri = QueryHelpers.AddQueryString(uri, Query.ToDictionary(i => i.Key, i => i.Value.ToString()));
            }

            return uri;
        }

        #endregion Overrides of Object
    }

    public class RequestMessageBuilder
    {
        private readonly HttpRequestMessage _requestMessage = new HttpRequestMessage();
        public UrlDescriptor UrlDescriptor { get; }

        public RequestMessageBuilder(UrlDescriptor urlDescriptor)
        {
            UrlDescriptor = urlDescriptor;
        }

        public RequestMessageBuilder Method(HttpMethod method)
        {
            _requestMessage.Method = method;
            return this;
        }

        public RequestMessageBuilder Scheme(string scheme)
        {
            UrlDescriptor.Scheme = scheme;
            return this;
        }

        public RequestMessageBuilder Host(string host)
        {
            UrlDescriptor.Host = host;
            return this;
        }

        public RequestMessageBuilder Port(int port)
        {
            UrlDescriptor.Port = port;
            return this;
        }

        public RequestMessageBuilder Path(string path)
        {
            UrlDescriptor.Path = path;
            return this;
        }

        public RequestMessageBuilder Query(string name, StringValues values)
        {
            var query = UrlDescriptor.Query;
            if (query.TryGetValue(name, out var current))
                values = StringValues.Concat(current, values);

            query[name] = values;

            return this;
        }

        public RequestMessageBuilder Header(string name, StringValues values)
        {
            var headers = (HttpHeaders)_requestMessage.Content?.Headers ?? _requestMessage.Headers;
            headers.Add(name, values.ToArray());

            return this;
        }

        public RequestMessageBuilder Body(byte[] data)
        {
            _requestMessage.Content = new ByteArrayContent(data);
            return this;
        }

        public RequestMessageBuilder Body(string content)
        {
            _requestMessage.Content = new StringContent(content);
            return this;
        }

        public HttpRequestMessage Build()
        {
            _requestMessage.RequestUri = new Uri(UrlDescriptor.ToString(), UriKind.Absolute);
            return _requestMessage;
        }

        public RequestMessageBuilder Property(string key, object value)
        {
            _requestMessage.Properties[key] = value;

            return this;
        }
    }
}