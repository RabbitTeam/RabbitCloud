using System;

namespace Rabbit.Rpc.Serialization
{
    /// <summary>
    /// 一个抽象的序列化器。
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// 序列化。
        /// </summary>
        /// <param name="instance">需要序列化的对象。</param>
        /// <returns>序列化之后的结果。</returns>
        byte[] Serialize(object instance);

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <param name="bytes">序列化的内容。</param>
        /// <param name="type">对象类型。</param>
        /// <returns>一个对象实例。</returns>
        object Deserialize(byte[] bytes, Type type);
    }

    /// <summary>
    /// 序列化器扩展方法。
    /// </summary>
    public static class SerializerExtensions
    {
        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="serializer">序列化器。</param>
        /// <param name="bytes">序列化的内容。</param>
        /// <returns>一个对象实例。</returns>
        public static T Deserialize<T>(this ISerializer serializer, byte[] bytes)
        {
            return (T)serializer.Deserialize(bytes, typeof(T));
        }
    }
}