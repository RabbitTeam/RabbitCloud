using System;
using System.Collections.Generic;

namespace RabbitCloud.Abstractions
{
    /// <summary>
    /// 属性字典表。
    /// </summary>
    public class AttributeDictionary
    {
        private readonly IDictionary<string, string> _dictionary;

        public AttributeDictionary()
        {
            _dictionary = new Dictionary<string, string>();
        }

        public AttributeDictionary(IDictionary<string, string> dictionary)
        {
            _dictionary = dictionary ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取所有属性。
        /// </summary>
        /// <returns>键值对应集合。</returns>
        public IEnumerable<KeyValuePair<string, string>> GetAttributes()
        {
            return _dictionary;
        }

        /// <summary>
        /// 获取一个属性。
        /// </summary>
        /// <param name="key">属性键。</param>
        /// <returns>属性值。</returns>
        public string Get(string key)
        {
            string value;
            _dictionary.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// 获取一个属性，如果属性不存在则返回 <paramref name="defaultValue"/>。
        /// </summary>
        /// <param name="key">属性键。</param>
        /// <param name="defaultValue">默认值。</param>
        /// <returns>属性值或默认值。</returns>
        public string Get(string key, string defaultValue)
        {
            string value;
            return _dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置一个属性。
        /// </summary>
        /// <param name="key">属性键。</param>
        /// <param name="value">属性值。</param>
        public void Set(string key, string value)
        {
            _dictionary[key] = value;
        }
    }
}