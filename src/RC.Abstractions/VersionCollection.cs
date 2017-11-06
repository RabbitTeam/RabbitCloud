using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Abstractions
{
    public interface IVersionCollection<in TKey, TItem> : IEnumerable<TItem>
    {
        /// <summary>
        /// Incremented for each modification and can be used to verify cached results.
        /// </summary>
        int Revision { get; }

        TItem this[TKey key] { get; set; }

        TItem Get(TKey key);

        void Set(TKey key, TItem item);
    }

    public class VersionCollection<TKey, TItem> : IVersionCollection<TKey, TItem>
    {
        private IDictionary<TKey, TItem> _items;
        private volatile int _containerRevision;

        #region Implementation of IEnumerable

        public IEnumerator<TItem> GetEnumerator()
        {
            return _items?.Values?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Implementation of IEnumerable

        #region Implementation of IVersionCollection<in TKey,TItem>

        /// <inheritdoc />
        /// <summary>
        /// Incremented for each modification and can be used to verify cached results.
        /// </summary>
        public int Revision => _containerRevision;

        public TItem this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                return _items != null && _items.TryGetValue(key, out var result) ? result : default(TItem);
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    if (_items != null && _items.Remove(key))
                    {
                        _containerRevision++;
                    }
                    return;
                }

                if (_items == null)
                {
                    _items = new Dictionary<TKey, TItem>();
                }
                _items[key] = value;
                _containerRevision++;
            }
        }

        public TItem Get(TKey key)
        {
            return this[key];
        }

        public void Set(TKey key, TItem item)
        {
            this[key] = item;
        }

        #endregion Implementation of IVersionCollection<in TKey,TItem>
    }
}