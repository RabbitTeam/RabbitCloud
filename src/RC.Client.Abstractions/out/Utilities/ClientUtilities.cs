using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Web;

namespace Rabbit.Cloud.Client.Abstractions.Utilities
{
    public static class ClientUtilities
    {
        public static Dictionary<string, StringValues> ParseNullableQuery(string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
                return null;

            if (queryString.Contains("?") && !queryString.StartsWith("?"))
            {
                queryString = queryString.Substring(queryString.IndexOf('?'));
            }

            var names = HttpUtility.ParseQueryString(queryString);

            if (!names.HasKeys())
                return null;

            var querys = new Dictionary<string, StringValues>(names.Count);

            foreach (string name in names)
            {
                var item = names[name];
                var values = item == null ? StringValues.Empty : new StringValues(item?.Split(','));

                querys[name] = values;
            }

            return querys;
        }

        public static Dictionary<string, StringValues> ParseQuery(string queryString)
        {
            return ParseNullableQuery(queryString) ?? new Dictionary<string, StringValues>();
        }

        public static (string Path, IDictionary<string, StringValues> Query) SplitPathAndQuery(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            var path = value;
            IDictionary<string, StringValues> query = null;

            var queryStartIndex = value.IndexOf('?');

            if (queryStartIndex != -1)
            {
                path = value.Substring(0, queryStartIndex);
                query = ParseNullableQuery(value.Substring(queryStartIndex));
            }

            return (path, query);
        }
    }
}