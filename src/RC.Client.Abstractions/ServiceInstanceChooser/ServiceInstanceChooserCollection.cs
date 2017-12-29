using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions.ServiceInstanceChooser
{
    public interface IServiceInstanceChooserCollection : IEnumerable<IServiceInstanceChooser>
    {
        IServiceInstanceChooser this[string name] { get; set; }

        IServiceInstanceChooser Get(string name);

        void Set(string name, IServiceInstanceChooser method);
    }

    public class ServiceInstanceChooserCollection : IServiceInstanceChooserCollection
    {
        private IDictionary<string, IServiceInstanceChooser> _items;

        #region Implementation of IEnumerable

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IServiceInstanceChooser> GetEnumerator()
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

        #region Implementation of IServiceInstanceChooserCollection

        public IServiceInstanceChooser this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException($"{nameof(name)} is empty.", nameof(name));

                return _items != null && _items.TryGetValue(name, out var method) ? method : null;
            }
            set
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException($"{nameof(name)} is empty.", nameof(name));

                if (_items != null && value == null)
                    _items.Remove(name);
                if (_items == null)
                    _items = new Dictionary<string, IServiceInstanceChooser>(StringComparer.OrdinalIgnoreCase);
                _items[name] = value;
            }
        }

        public IServiceInstanceChooser Get(string name)
        {
            return this[name];
        }

        public void Set(string name, IServiceInstanceChooser method)
        {
            this[name] = method;
        }

        #endregion Implementation of IServiceInstanceChooserCollection
    }
}