using System;

namespace RabbitCloud.Rpc.Abstractions.Codec
{
    /// <summary>
    /// 一个抽象的编解码器。
    /// </summary>
    public interface ICodec
    {
        /// <summary>
        /// 对内容进行编码。
        /// </summary>
        /// <param name="content">内容。</param>
        /// <returns>编码后的结果。</returns>
        object Encode(object content);

        /// <summary>
        /// 对内容进行解码。
        /// </summary>
        /// <param name="content">内容。</param>
        /// <param name="type">内容类型。</param>
        /// <returns>解码后的结果。</returns>
        object Decode(object content, Type type);
    }
}