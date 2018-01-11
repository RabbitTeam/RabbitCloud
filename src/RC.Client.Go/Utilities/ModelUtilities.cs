using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Go.Abstractions;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.Go.Utilities
{
    public static class ModelUtilities
    {
        public static IDictionary<string, StringValues> GetRequestHeaders(this RequestModel requestModel)
        {
            var providers = GetRequestAttributes(requestModel)
                .OfType<IHeadersProvider>()
                .ToArray();

            return MergeHeaders(null, providers);
        }

        public static IDictionary<object, object> GetRequestItems(this RequestModel requestModel)
        {
            var providers = GetRequestAttributes(requestModel)
                .OfType<IItemsProvider>()
                .ToArray();
            return MergeItems(null, providers);
        }

        public static IDictionary<string, StringValues> MergeHeaders(IDictionary<string, StringValues> headers, IReadOnlyList<IHeadersProvider> providers)
        {
            if (providers == null || !providers.Any())
                return null;

            if (headers == null)
                headers = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providers)
            {
                var itemHeaders = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
                provider.Collect(itemHeaders);

                foreach (var itemHeader in itemHeaders)
                {
                    var values = headers.TryGetValue(itemHeader.Key, out var item) ? StringValues.Concat(item, itemHeader.Value) : itemHeader.Value;
                    headers[itemHeader.Key] = values;
                }
            }

            return headers;
        }

        public static IEnumerable<object> GetRequestAttributes(this RequestModel requestModel)
        {
            return requestModel.ServiceModel.Attributes.Concat(requestModel.Attributes)
                .Concat(requestModel.Parameters.SelectMany(p => p.Attributes));
        }

        public static IDictionary<object, object> MergeItems(IDictionary<object, object> items, IReadOnlyList<IItemsProvider> providers)
        {
            if (providers == null || !providers.Any())
                return null;

            if (items == null)
                items = new Dictionary<object, object>();

            foreach (var provider in providers)
                provider.Collect(items);
            return items;
        }
    }
}