using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Features
{
    /*    internal static class QueryHelper
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

        public struct ServiceUrl
        {
            private string _full;
            private IReadOnlyDictionary<string, StringValues> _query;

            public ServiceUrl(string url)
            {
                if (string.IsNullOrEmpty(url))
                    throw new ArgumentException("url is null or empty.", nameof(url));

                var builder = new UriBuilder(url);

                Scheme = builder.Scheme;
                Host = builder.Host;
                Port = builder.Port;
                Path = builder.Path;
                QueryString = builder.Query;
                Fragment = builder.Fragment;

                if (string.IsNullOrEmpty(builder.Uri.UserInfo))
                {
                    Authority = builder.Uri.Authority;
                }
                else
                {
                    Authority = builder.Uri.UserInfo + "@" + builder.Uri.Authority;
                }

                _full = builder.ToString();
                _query = null;
            }

            #region Property

            public string Scheme { get; }
            public string Authority { get; }
            public string Host { get; }
            public int Port { get; }
            public string Path { get; }
            public string QueryString { get; }

            public IReadOnlyDictionary<string, StringValues> Query
            {
                get
                {
                    if (_query != null)
                        return _query;
                    return _query = QueryHelper.ParseQuery(QueryString);
                }
            }

            public string Fragment { get; }

            #endregion Property

            #region Overrides of Object

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString()
            {
                return _full ?? (_full = BuildString());
            }

            #endregion Overrides of Object

            #region Private Method

            private string BuildString()
            {
                var builder = new StringBuilder();

                builder.Append($"{Scheme}://{Authority}");
                builder.Append(Path);
                if (!string.IsNullOrEmpty(QueryString))
                    builder.Append(QueryString);
                if (!string.IsNullOrEmpty(Fragment))
                    builder.Append(Fragment);

                return builder.ToString();
            }

            #endregion Private Method
        }*/

    public interface IRequestFeature
    {
        string Scheme { get; set; }
        string Host { get; set; }
        int Port { get; set; }
        string Path { get; set; }
        string QueryString { get; set; }
        IDictionary<string, StringValues> Headers { get; set; }
        object Body { get; set; }
    }
}