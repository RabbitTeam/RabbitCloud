using System;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的编解码器。
    /// </summary>
    public interface ICodec
    {
        /// <summary>
        /// 对消息进行编码。
        /// </summary>
        /// <param name="message">消息实例。</param>
        /// <returns>编码后的内容。</returns>
        object Encode(object message);

        /// <summary>
        /// 对消息进行解码。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <param name="type">内容类型。</param>
        /// <returns>解码后的内容。</returns>
        object Decode(object message, Type type);
    }
}