using Rabbit.Cloud.Client.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Internal.Serialization
{
    public class SerializerTable : IEnumerable<ISerializer>
    {
        private IDictionary<string, ISerializer> _items;

        public ISerializer this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("nameof(fullName) is empty.", nameof(name));

                return _items != null && _items.TryGetValue(name, out var method) ? method : null;
            }
            set
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("nameof(fullName) is empty.", nameof(name));

                if (_items != null && value == null)
                    _items.Remove(name);
                if (_items == null)
                    _items = new Dictionary<string, ISerializer>(StringComparer.OrdinalIgnoreCase);
                _items[name] = value;
            }
        }

        public ISerializer Get(string name)
        {
            return this[name];
        }

        public void Set(string name, ISerializer serializer)
        {
            this[name] = serializer;
        }

        #region Implementation of IEnumerable

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ISerializer> GetEnumerator()
        {
            return _items?.Values?.GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Implementation of IEnumerable
    }
}