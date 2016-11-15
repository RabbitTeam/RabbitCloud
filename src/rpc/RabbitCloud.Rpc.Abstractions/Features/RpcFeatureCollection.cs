using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Rpc.Abstractions.Features
{
    /// <summary>
    /// 一个抽象的Rpc特性集合。
    /// </summary>
    public interface IRpcFeatureCollection : IEnumerable<KeyValuePair<Type, object>>
    {
        /// <summary>
        /// 是否只读。
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// 修改次数。
        /// </summary>
        int Revision { get; }

        /// <summary>
        /// 根据key得到一个特性。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>特性实例。</returns>
        object this[Type key] { get; set; }

        /// <summary>
        /// 获取一个指定类型的特性实例。
        /// </summary>
        /// <typeparam name="TFeature">特性类型。</typeparam>
        /// <returns>特性实例。</returns>
        TFeature Get<TFeature>();

        /// <summary>
        /// 设置一个特性。
        /// </summary>
        /// <typeparam name="TFeature">特性类型。</typeparam>
        /// <param name="instance">特性实例。</param>
        void Set<TFeature>(TFeature instance);
    }

    public class RpcFeatureCollection : IRpcFeatureCollection
    {
        #region Field

        private static readonly KeyComparer FeatureKeyComparer = new KeyComparer();
        private readonly IRpcFeatureCollection _defaults;
        private IDictionary<Type, object> _features;
        private volatile int _containerRevision;

        #endregion Field

        #region Constructor

        public RpcFeatureCollection()
        {
        }

        public RpcFeatureCollection(IRpcFeatureCollection defaults)
        {
            _defaults = defaults;
        }

        #endregion Constructor

        #region Implementation of IEnumerable

        /// <summary>返回一个循环访问集合的枚举器。</summary>
        /// <returns>用于循环访问集合的枚举数。</returns>
        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            if (_features != null)
            {
                foreach (var pair in _features)
                {
                    yield return pair;
                }
            }

            if (_defaults != null)
            {
                // Don't return features masked by the wrapper.
                foreach (var pair in _features == null ? _defaults : _defaults.Except(_features, FeatureKeyComparer))
                {
                    yield return pair;
                }
            }
        }

        /// <summary>返回循环访问集合的枚举数。</summary>
        /// <returns>可用于循环访问集合的 <see cref="T:System.Collections.IEnumerator" /> 对象。</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Implementation of IEnumerable

        #region Implementation of IRpcFeatureCollection

        /// <summary>
        /// 是否只读。
        /// </summary>
        public bool IsReadOnly { get; } = false;

        /// <summary>
        /// 修改次数。
        /// </summary>
        public int Revision => _containerRevision + (_defaults?.Revision ?? 0);

        /// <summary>
        /// 根据key得到一个特性。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>特性实例。</returns>
        public object this[Type key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                object result;
                return _features != null && _features.TryGetValue(key, out result) ? result : _defaults?[key];
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

        /// <summary>
        /// 获取一个指定类型的特性实例。
        /// </summary>
        /// <typeparam name="TFeature">特性类型。</typeparam>
        /// <returns>特性实例。</returns>
        public TFeature Get<TFeature>()
        {
            return (TFeature)this[typeof(TFeature)];
        }

        /// <summary>
        /// 设置一个特性。
        /// </summary>
        /// <typeparam name="TFeature">特性类型。</typeparam>
        /// <param name="instance">特性实例。</param>
        public void Set<TFeature>(TFeature instance)
        {
            this[typeof(TFeature)] = instance;
        }

        #endregion Implementation of IRpcFeatureCollection

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