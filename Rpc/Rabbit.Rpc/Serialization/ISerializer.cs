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
        /// <param name="obj">需要序列化的对象。</param>
        /// <returns>序列化之后的结果。</returns>
        string Serialize(object obj);

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="content">序列化的内容。</param>
        /// <returns>一个对象实例。</returns>
        T Deserialize<T>(string content);
    }
}