using Newtonsoft.Json;

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
        /// <param name="obj">需要序列化的对象。</param>
        /// <returns>序列化之后的结果。</returns>
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="content">序列化的内容。</param>
        /// <returns>一个对象实例。</returns>
        public T Deserialize<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }

        #endregion Implementation of ISerializer
    }
}