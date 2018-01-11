using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go.Abstractions
{
    public class GoRequestContext
    {
        public GoRequestContext(IRabbitContext rabbitContext)
        {
            RabbitContext = rabbitContext;
        }

        public GoRequestContext(GoRequestContext goRequestContext)
            : this(goRequestContext.RabbitContext)
        {
            Arguments = goRequestContext.Arguments;
            RequestUrl = goRequestContext.RequestUrl;
            PathVariables = goRequestContext.PathVariables;
        }

        public IRabbitContext RabbitContext { get; }
        public IDictionary<string, object> Arguments { get; set; }
        public string RequestUrl { get; set; }
        public IDictionary<string, object> PathVariables { get; set; }

        public GoRequestContext AppendQuery(IDictionary<string, StringValues> values)
        {
            if (values == null)
                return this;

            foreach (var item in values)
            {
                AppendQuery(item.Key, item.Value);
            }

            return this;
        }

        public GoRequestContext AppendQuery(string key, StringValues value)
        {
            var query = RabbitContext.Request.Query;

            value = query.TryGetValue(key, out var temp) ? StringValues.Concat(temp, value) : value;
            query[key] = value;

            return this;
        }

        public GoRequestContext AppendHeaders(IDictionary<string, StringValues> values)
        {
            if (values == null)
                return this;

            foreach (var item in values)
            {
                AppendHeaders(item.Key, item.Value);
            }

            return this;
        }

        public GoRequestContext AppendHeaders(string key, StringValues value)
        {
            var headers = RabbitContext.Request.Headers;

            value = headers.TryGetValue(key, out var temp) ? StringValues.Concat(temp, value) : value;
            headers[key] = value;

            return this;
        }

        public GoRequestContext AppendItems(IDictionary<object, object> values)
        {
            if (values == null)
                return this;

            var items = RabbitContext.Items;
            foreach (var item in values)
            {
                items[item.Key] = item.Value;
            }

            return this;
        }

        public GoRequestContext AppendItems(object key, object value)
        {
            var items = RabbitContext.Items;

            items[key] = value;

            return this;
        }

        public GoRequestContext AppendPathVariable(IDictionary<string, StringValues> values)
        {
            if (values == null)
                return this;

            foreach (var item in values)
            {
                AppendPathVariable(item.Key, item.Value);
            }

            return this;
        }

        public GoRequestContext AppendPathVariable(string key, string value)
        {
            if (PathVariables == null)
                PathVariables = new Dictionary<string, object>();
            PathVariables[key] = value;
            return this;
        }

        public GoRequestContext SetBody(object body)
        {
            RabbitContext.Request.Body = body;

            return this;
        }
    }
}