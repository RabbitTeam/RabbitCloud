namespace Rabbit.Rpc.Transport
{
    /// <summary>
    /// 传输消息模型。
    /// </summary>
    public class TransportMessage
    {
        /// <summary>
        /// 消息Id。
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 消息内容。
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// 将泛型的TransportMessage转换成非泛型的TransportMessage。
        /// </summary>
        /// <typeparam name="T">内容类型。</typeparam>
        /// <param name="message">消息实例。</param>
        /// <returns>消息实例。</returns>
        public static TransportMessage Convert<T>(TransportMessage<T> message)
        {
            return new TransportMessage
            {
                Id = message.Id,
                Content = message.Content
            };
        }
    }

    /// <summary>
    /// 一个泛型传输消息模型。
    /// </summary>
    /// <typeparam name="T">消息内容类型。</typeparam>
    public class TransportMessage<T>
    {
        /// <summary>
        /// 消息Id。
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 消息内容。
        /// </summary>
        public T Content { get; set; }
    }
}