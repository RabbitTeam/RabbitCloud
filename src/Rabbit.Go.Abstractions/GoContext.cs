using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions
{
    public delegate Task GoRequestDelegate(GoContext context);

    public class GoContext
    {
        public GoContext()
        {
            Request = new GoRequest(this);
            Response = new GoResponse(this);
            Items = new Dictionary<object, object>();
        }

        public GoRequest Request { get; }
        public GoResponse Response { get; }
        public IServiceProvider RequestServices { get; set; }
        public IDictionary<object, object> Items { get; set; }
    }

    public class GoRequest
    {
        public GoRequest(GoContext goContext)
        {
            GoContext = goContext;
            Headers = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            Query = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        public GoContext GoContext { get; }
        public string Method { get; set; }
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

        public IDictionary<string, StringValues> Headers { get; }
        public IDictionary<string, StringValues> Query { get; }
        public Stream Body { get; set; }
    }

    public static class GoRequestExtensions
    {
        public static GoRequest Query(this GoRequest request, string name, StringValues values)
        {
            var query = request.Query;
            query[name] = values;

            return request;
        }

        public static GoRequest Header(this GoRequest request, string name, StringValues values)
        {
            var headers = request.Headers;

            headers.Remove(name);
            headers.Add(name, values.ToArray());

            return request;
        }

        public static GoRequest AddQuery(this GoRequest request, string name, StringValues values)
        {
            var query = request.Query;
            if (query.TryGetValue(name, out var current))
                values = StringValues.Concat(current, values);

            query[name] = values;

            return request;
        }

        public static GoRequest AddHeader(this GoRequest request, string name, StringValues values)
        {
            var headers = request.Headers;

            headers.Add(name, values.ToArray());

            return request;
        }

        public static GoRequest Body(this GoRequest request, byte[] data, string contentType = "application/json")
        {
            request.Body = new MemoryStream(data);
            return request.Header("Content-Type", contentType);
        }

        public static GoRequest Body(this GoRequest request, string content, string contentType = "text/plain")
        {
            return request.Body(Encoding.UTF8.GetBytes(content), contentType);
        }
    }

    public class GoResponse
    {
        public GoResponse(GoContext goContext)
        {
            GoContext = goContext;
            Headers = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        public GoContext GoContext { get; }
        public Stream Content { get; set; }
        public IDictionary<string, StringValues> Headers { get; }
        public int StatusCode { get; set; }
    }
}