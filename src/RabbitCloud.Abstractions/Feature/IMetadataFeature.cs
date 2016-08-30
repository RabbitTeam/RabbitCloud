using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Abstractions.Feature
{
    /// <summary>
    /// 元数据功能。
    /// </summary>
    public interface IMetadataFeature
    {
        /// <summary>
        /// 元数据。
        /// </summary>
        IDictionary<string, object> Metadata { get; }
    }

    /// <summary>
    /// 元数据功能扩展方法。
    /// </summary>
    public static class MetadataFeatureExtensions
    {
        /// <summary>
        /// 获取一个元数据。
        /// </summary>
        /// <typeparam name="T">元数据类型。</typeparam>
        /// <param name="metadataFeature">元数据功能。</param>
        /// <param name="name">元数据名称。</param>
        /// <param name="def">如果指定名称的元数据不存在则返回这个参数。</param>
        /// <returns>元数据值。</returns>
        public static T GetMetadata<T>(this IMetadataFeature metadataFeature, string name, T def = default(T))
        {
            if (metadataFeature == null)
                throw new ArgumentNullException(nameof(metadataFeature));

            var metadata = metadataFeature.Metadata;
            if (metadata == null)
                return def;
            return !metadata.ContainsKey(name) ? def : (T)metadata[name];
        }

        /// <summary>
        /// 设置一个元数据。
        /// </summary>
        /// <param name="metadataFeature">元数据功能。</param>
        /// <param name="name">元数据名称。</param>
        /// <param name="value">元数据值。</param>
        public static void SetMetadata(this IMetadataFeature metadataFeature, string name, object value)
        {
            if (metadataFeature == null)
                throw new ArgumentNullException(nameof(metadataFeature));
            if (metadataFeature.Metadata == null)
                throw new ArgumentNullException(nameof(metadataFeature.Metadata));

            var metadata = metadataFeature.Metadata;

            metadata[name] = value;
        }

        /// <summary>
        /// 将 <paramref name="sourceMetadataFeature"/> 中的数据项添加至 <paramref name="metadataFeature"/>，如果 <paramref name="metadataFeature"/> 中已经存在则跳过。
        /// </summary>
        /// <param name="metadataFeature">目标元数据。</param>
        /// <param name="sourceMetadataFeature">源元数据。</param>
        public static void ComposeMetadata(this IMetadataFeature metadataFeature, IMetadataFeature sourceMetadataFeature)
        {
            foreach (var source in sourceMetadataFeature.Metadata.Where(i => !metadataFeature.Metadata.ContainsKey(i.Key)))
            {
                metadataFeature.SetMetadata(source.Key, source.Value);
            }
        }
    }
}