using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Features
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a wrapper for RequestHeaders and ResponseHeaders.
    /// </summary>
    internal sealed class HeaderDictionary : IDictionary<string, StringValues>
    {
        private static readonly string[] EmptyKeys = Array.Empty<string>();
        private static readonly StringValues[] EmptyValues = Array.Empty<StringValues>();
        private static readonly Enumerator EmptyEnumerator = new Enumerator();

        // Pre-box
        private static readonly IEnumerator<KeyValuePair<string, StringValues>> EmptyIEnumeratorType = EmptyEnumerator;

        private static readonly IEnumerator EmptyIEnumerator = EmptyEnumerator;

        public HeaderDictionary()
        {
        }

        public HeaderDictionary(Dictionary<string, StringValues> store)
        {
            Store = store;
        }

        public HeaderDictionary(int capacity)
        {
            EnsureStore(capacity);
        }

        private Dictionary<string, StringValues> Store { get; set; }

        private void EnsureStore(int capacity)
        {
            if (Store == null)
            {
                Store = new Dictionary<string, StringValues>(capacity, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Get or sets the associated value from the collection as a single string.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <returns>the associated value from the collection as a StringValues or StringValues.Empty if the key is not present.</returns>
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
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                ThrowIfReadOnly();

                if (StringValues.IsNullOrEmpty(value))
                {
                    Store?.Remove(key);
                }
                else
                {
                    EnsureStore(1);
                    Store[key] = value;
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Throws KeyNotFoundException if the key is not present.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <returns></returns>
        StringValues IDictionary<string, StringValues>.this[string key]
        {
            get => Store[key];
            set
            {
                ThrowIfReadOnly();
                this[key] = value;
            }
        }

        public long? ContentLength
        {
            get
            {
                var rawValue = this[HeaderNames.ContentLength];
                if (rawValue.Count == 1 &&
                    !string.IsNullOrEmpty(rawValue[0]) &&
                    HeaderUtilities.TryParseNonNegativeInt64(new StringSegment(rawValue[0]).Trim(), out var value))
                {
                    return value;
                }

                return null;
            }
            set
            {
                ThrowIfReadOnly();
                if (value.HasValue)
                {
                    this[HeaderNames.ContentLength] = HeaderUtilities.FormatNonNegativeInt64(value.Value);
                }
                else
                {
                    Remove(HeaderNames.ContentLength);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:Rabbit.Cloud.Application.Features.HeaderDictionary" />;.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:Rabbit.Cloud.Application.Features.HeaderDictionary" />.</returns>
        public int Count => Store?.Count ?? 0;

        /// <inheritdoc />
        /// <summary>
        /// Gets a value that indicates whether the <see cref="T:Rabbit.Cloud.Application.Features.HeaderDictionary" /> is in read-only mode.
        /// </summary>
        /// <returns>true if the <see cref="T:Rabbit.Cloud.Application.Features.HeaderDictionary" /> is in read-only mode; otherwise, false.</returns>
        public bool IsReadOnly { get; set; }

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

        public ICollection<StringValues> Values
        {
            get
            {
                if (Store == null)
                {
                    return EmptyValues;
                }
                return Store.Values;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds a new list of items to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(KeyValuePair<string, StringValues> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentNullException("The key is null");
            }
            ThrowIfReadOnly();
            EnsureStore(1);
            Store.Add(item.Key, item.Value);
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds the given header and values to the collection.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <param name="value">The header values.</param>
        public void Add(string key, StringValues value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            ThrowIfReadOnly();
            EnsureStore(1);
            Store.Add(key, value);
        }

        /// <inheritdoc />
        /// <summary>
        /// Clears the entire list of objects.
        /// </summary>
        public void Clear()
        {
            ThrowIfReadOnly();
            Store?.Clear();
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a value indicating whether the specified object occurs within this collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>true if the specified object occurs within this collection; otherwise, false.</returns>
        public bool Contains(KeyValuePair<string, StringValues> item)
        {
            return Store != null && Store.TryGetValue(item.Key, out var value) && StringValues.Equals(value, item.Value);
        }

        /// <inheritdoc />
        /// <summary>
        /// Determines whether the <see cref="T:Rabbit.Cloud.Application.Features.HeaderDictionary" /> contains a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the <see cref="T:Rabbit.Cloud.Application.Features.HeaderDictionary" /> contains a specific key; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            return Store != null && Store.ContainsKey(key);
        }

        /// <inheritdoc />
        /// <summary>
        /// Copies the <see cref="T:Rabbit.Cloud.Application.Features.HeaderDictionary" /> elements to a one-dimensional Array instance at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the specified objects copied from the <see cref="T:Rabbit.Cloud.Application.Features.HeaderDictionary" />.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
        {
            if (Store == null)
            {
                return;
            }

            foreach (var item in Store)
            {
                array[arrayIndex] = item;
                arrayIndex++;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Removes the given item from the the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>true if the specified object was removed from the collection; otherwise, false.</returns>
        public bool Remove(KeyValuePair<string, StringValues> item)
        {
            ThrowIfReadOnly();
            if (Store == null)
            {
                return false;
            }

            if (Store.TryGetValue(item.Key, out var value) && StringValues.Equals(item.Value, value))
            {
                return Store.Remove(item.Key);
            }
            return false;
        }

        /// <inheritdoc />
        /// <summary>
        /// Removes the given header from the collection.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <returns>true if the specified object was removed from the collection; otherwise, false.</returns>
        public bool Remove(string key)
        {
            ThrowIfReadOnly();
            return Store != null && Store.Remove(key);
        }

        /// <inheritdoc />
        /// <summary>
        /// Retrieves a value from the dictionary.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the <see cref="T:Rabbit.Cloud.Application.Features.HeaderDictionary" /> contains the key; otherwise, false.</returns>
        public bool TryGetValue(string key, out StringValues value)
        {
            if (Store != null) return Store.TryGetValue(key, out value);
            value = default(StringValues);
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="Enumerator" /> object that can be used to iterate through the collection.</returns>
        public Enumerator GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                // Non-boxed Enumerator
                return EmptyEnumerator;
            }
            return new Enumerator(Store.GetEnumerator());
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<string, StringValues>> IEnumerable<KeyValuePair<string, StringValues>>.GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                // Non-boxed Enumerator
                return EmptyIEnumeratorType;
            }
            return Store.GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                // Non-boxed Enumerator
                return EmptyIEnumerator;
            }
            return Store.GetEnumerator();
        }

        private void ThrowIfReadOnly()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("The response headers cannot be modified because the response has already started.");
            }
        }

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
    }

    public class RequestFeature : IRequestFeature
    {
        public RequestFeature()
        {
            Headers = new HeaderDictionary();
            Scheme = Host = Path = QueryString = string.Empty;
            Port = -1;
        }

        #region Implementation of IRequestFeature

        public string Scheme { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        private string _path;

        public string Path
        {
            get => _path;
            set
            {
                _path = string.IsNullOrEmpty(value) ? "/" : value;

                if (!_path.StartsWith("/"))
                    _path = "/" + _path;
            }
        }

        private string _queryString;

        public string QueryString
        {
            get => _queryString;
            set
            {
                _queryString = string.IsNullOrEmpty(value) ? string.Empty : value;

                if (!_queryString.StartsWith("?"))
                    _queryString = "?" + _queryString;
            }
        }

        public IDictionary<string, StringValues> Headers { get; set; }
        public object Body { get; set; }
        public Type BodyType { get; set; }

        #endregion Implementation of IRequestFeature
    }
}