namespace Rabbit.Rpc.Messages
{
    /// <summary>
    /// 传输消息模型。
    /// </summary>
    public abstract class TransportMessage
    {
        /// <summary>
        /// 消息Id。
        /// </summary>
        public string Id { get; set; }
    }
}