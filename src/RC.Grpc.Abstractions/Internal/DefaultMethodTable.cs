using Grpc.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Abstractions.Internal
{
    public class DefaultMethodTable : IMethodTable
    {
        private IDictionary<string, IMethod> _items;

        #region Implementation of IMethodTable

        public IMethod this[string fullName]
        {
            get
            {
                if (fullName == null)
                    throw new ArgumentNullException(nameof(fullName));
                if (string.IsNullOrEmpty(fullName))
                    throw new ArgumentException("nameof(fullName) is empty.", nameof(fullName));

                return _items != null && _items.TryGetValue(fullName, out var method) ? method : null;
            }
            set
            {
                if (fullName == null)
                    throw new ArgumentNullException(nameof(fullName));
                if (string.IsNullOrEmpty(fullName))
                    throw new ArgumentException("nameof(fullName) is empty.", nameof(fullName));

                if (_items != null && value == null)
                    _items.Remove(fullName);
                if (_items == null)
                    _items = new Dictionary<string, IMethod>(StringComparer.OrdinalIgnoreCase);
                _items[fullName] = value;
            }
        }

        public IMethod Get(string fullName)
        {
            return this[fullName];
        }

        public void Set(IMethod method)
        {
            this[method.FullName] = method;
        }

        #endregion Implementation of IMethodTable

        #region Implementation of IEnumerable

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IMethod> GetEnumerator()
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