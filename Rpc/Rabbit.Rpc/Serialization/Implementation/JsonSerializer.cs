using Newtonsoft.Json;
using System;
using System.Text;

namespace Rabbit.Rpc.Serialization.Implementation
{
    /// <summary>
    /// Json序列化器。
    /// </summary>
    public sealed class JsonSerializer : ISerializer
    {
        #region Implementation of ISerializer

        /// <summary>
        /// 序列化。
        /// </summary>
        /// <param name="instance">需要序列化的对象。</param>
        /// <returns>序列化之后的结果。</returns>
        public byte[] Serialize(object instance)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(instance));
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <param name="bytes">序列化的内容。</param>
        /// <param name="type">对象类型。</param>
        /// <returns>一个对象实例。</returns>
        public object Deserialize(byte[] bytes, Type type)
        {
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), type);
        }

        #endregion Implementation of ISerializer
    }
}