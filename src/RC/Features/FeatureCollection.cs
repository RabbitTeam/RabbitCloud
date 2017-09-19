using RC.Abstractions.Features;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Features
{
    public class FeatureCollection : IFeatureCollection
    {
        private static readonly KeyComparer FeatureKeyComparer = new KeyComparer();
        private readonly IFeatureCollection _defaults;
        private IDictionary<Type, object> _features;
        private volatile int _containerRevision;

        public FeatureCollection()
        {
        }

        public FeatureCollection(IFeatureCollection defaults)
        {
            _defaults = defaults;
        }

        #region Implementation of IEnumerable

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            if (_features != null)
            {
                foreach (var pair in _features)
                {
                    yield return pair;
                }
            }

            if (_defaults == null)
                yield break;

            // Don't return features masked by the wrapper.
            foreach (var pair in _features == null ? _defaults : _defaults.Except(_features, FeatureKeyComparer))
            {
                yield return pair;
            }
        }

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Implementation of IEnumerable

        #region Implementation of IFeatureCollection

        /// <inheritdoc />
        /// <summary>
        /// Indicates if the collection can be modified.
        /// </summary>
        public bool IsReadOnly => false;

        /// <inheritdoc />
        /// <summary>
        /// Incremented for each modification and can be used to verify cached results.
        /// </summary>
        public int Revision => _containerRevision + (_defaults?.Revision ?? 0);

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets a given feature. Setting a null value removes the feature.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The requested feature, or null if it is not present.</returns>
        public object this[Type key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                return _features != null && _features.TryGetValue(key, out var result) ? result : _defaults?[key];
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    if (_features != null && _features.Remove(key))
                    {
                        _containerRevision++;
                    }
                    return;
                }

                if (_features == null)
                {
                    _features = new Dictionary<Type, object>();
                }
                _features[key] = value;
                _containerRevision++;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Retrieves the requested feature from the collection.
        /// </summary>
        /// <typeparam name="TFeature">The feature key.</typeparam>
        /// <returns>The requested feature, or null if it is not present.</returns>
        public TFeature Get<TFeature>()
        {
            return (TFeature)this[typeof(TFeature)];
        }

        /// <inheritdoc />
        /// <summary>
        /// Sets the given feature in the collection.
        /// </summary>
        /// <typeparam name="TFeature">The feature key.</typeparam>
        /// <param name="instance">The feature value.</param>
        public void Set<TFeature>(TFeature instance)
        {
            this[typeof(TFeature)] = instance;
        }

        #endregion Implementation of IFeatureCollection

        private class KeyComparer : IEqualityComparer<KeyValuePair<Type, object>>
        {
            public bool Equals(KeyValuePair<Type, object> x, KeyValuePair<Type, object> y)
            {
                return x.Key == y.Key;
            }

            public int GetHashCode(KeyValuePair<Type, object> obj)
            {
                return obj.Key.GetHashCode();
            }
        }
    }
}