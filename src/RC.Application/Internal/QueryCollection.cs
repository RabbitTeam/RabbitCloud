using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application.Features;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Internal
{
    public class QueryCollection : IQueryCollection
    {
        public static readonly QueryCollection Empty = new QueryCollection();
        private static readonly string[] EmptyKeys = Array.Empty<string>();
        private static readonly Enumerator EmptyEnumerator = new Enumerator();

        // Pre-box
        private static readonly IEnumerator<KeyValuePair<string, StringValues>> EmptyIEnumeratorType = EmptyEnumerator;

        private static readonly IEnumerator EmptyIEnumerator = EmptyEnumerator;

        private Dictionary<string, StringValues> Store { get; }

        public QueryCollection()
        {
        }

        public QueryCollection(Dictionary<string, StringValues> store)
        {
            Store = store;
        }

        public QueryCollection(QueryCollection store)
        {
            Store = store.Store;
        }

        public QueryCollection(int capacity)
        {
            Store = new Dictionary<string, StringValues>(capacity, StringComparer.OrdinalIgnoreCase);
        }

        #region Implementation of IEnumerable

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