using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using System;
using System.Collections;
using System.Collections.Generic;
using StringValues = Microsoft.Extensions.Primitives.StringValues;

namespace Rabbit.Go.Abstractions
{
    public class RequestContext
    {
        public RequestContext(RequestContext requestContext)
        {
            RabbitContext = requestContext.RabbitContext;
            Arguments = requestContext.Arguments;
        }

        public RequestContext(IRabbitContext rabbitContext, IDictionary<string, object> arguments)
        {
            RabbitContext = rabbitContext;
            Arguments = arguments;
            rabbitContext.Request.Query = _query = new GoQueryCollection(rabbitContext.Request.Query);
        }

        public IRabbitContext RabbitContext { get; }

        public IDictionary<string, object> Arguments { get; set; }
//        public string Uri { get; set; }

        public IDictionary<string, StringValues> PathVariables { get; private set; }

        private readonly GoQueryCollection _query;

        public RequestContext AddQuery(string key, StringValues values)
        {
            _query.Add(key, values);

            return this;
        }

        public RequestContext AddHeader(string key, StringValues values)
        {
            var headers = RabbitContext.Request.Headers;
            values = headers.TryGetValue(key, out var current) ? StringValues.Concat(current, values) : values;
            headers[key] = values;

            return this;
        }

        public RequestContext AddPathVariable(string key, StringValues values)
        {
            if (PathVariables == null)
                PathVariables = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

            var pathVariables = PathVariables;
            values = pathVariables.TryGetValue(key, out var current) ? StringValues.Concat(current, values) : values;
            pathVariables[key] = values;

            return this;
        }

        public RequestContext SetItem(object key, object value)
        {
            RabbitContext.Items[key] = value;

            return this;
        }

        public RequestContext SetBody(object body)
        {
            RabbitContext.Request.Body = body;

            return this;
        }
    }

    public class GoQueryCollection : IQueryCollection
    {
        private static readonly string[] EmptyKeys = Array.Empty<string>();
        private static readonly Enumerator EmptyEnumerator = new Enumerator();

        // Pre-box
        private static readonly IEnumerator<KeyValuePair<string, StringValues>> EmptyIEnumeratorType = EmptyEnumerator;

        private static readonly IEnumerator EmptyIEnumerator = EmptyEnumerator;

        private Dictionary<string, StringValues> Store { get; }

        public GoQueryCollection(IQueryCollection queryCollection)
        {
            Store = new Dictionary<string, StringValues>(queryCollection.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var item in queryCollection)
                Store[item.Key] = item.Value;
        }

        public void Add(string key, StringValues values)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(key);

            values = Store.TryGetValue(key, out var current) ? StringValues.Concat(current, values) : values;
            Store[key] = values;
        }

        #region Implementation of IEnumerable

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                // Non-boxed Enumerator
                return EmptyIEnumeratorType;
            }
            return Store.GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                // Non-boxed Enumerator
                return EmptyIEnumerator;
            }
            return Store.GetEnumerator();
        }

        #endregion Implementation of IEnumerable

        #region Implementation of IQueryCollection

        public int Count => Store?.Count ?? 0;

        public ICollection<string> Keys
        {
            get
            {
                if (Store == null)
                {
                    return EmptyKeys;
                }
                return Store.Keys;
            }
        }

        public bool ContainsKey(string key)
        {
            return Store != null && Store.ContainsKey(key);
        }

        public bool TryGetValue(string key, out StringValues value)
        {
            if (Store != null) return Store.TryGetValue(key, out value);
            value = default(StringValues);
            return false;
        }

        public StringValues this[string key]
        {
            get
            {
                if (Store == null)
                {
                    return StringValues.Empty;
                }

                return TryGetValue(key, out var value) ? value : StringValues.Empty;
            }
        }

        #endregion Implementation of IQueryCollection

        #region Help Type

        public struct Enumerator : IEnumerator<KeyValuePair<string, StringValues>>
        {
            // Do NOT make this readonly, or MoveNext will not work
            private Dictionary<string, StringValues>.Enumerator _dictionaryEnumerator;

            private readonly bool _notEmpty;

            internal Enumerator(Dictionary<string, StringValues>.Enumerator dictionaryEnumerator)
            {
                _dictionaryEnumerator = dictionaryEnumerator;
                _notEmpty = true;
            }

            public bool MoveNext()
            {
                return _notEmpty && _dictionaryEnumerator.MoveNext();
            }

            public KeyValuePair<string, StringValues> Current => _notEmpty ? _dictionaryEnumerator.Current : default(KeyValuePair<string, StringValues>);

            public void Dispose()
            {
            }

            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                if (_notEmpty)
                {
                    ((IEnumerator)_dictionaryEnumerator).Reset();
                }
            }
        }

        #endregion Help Type
    }
}