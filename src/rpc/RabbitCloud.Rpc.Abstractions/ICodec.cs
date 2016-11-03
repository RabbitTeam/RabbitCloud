using System;
using System.IO;
using System.Text;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的编解码器。
    /// </summary>
    public interface ICodec
    {
        /// <summary>
        /// 编码。
        /// </summary>
        /// <param name="writer">写入器。</param>
        /// <param name="message">消息。</param>
        void Encode(TextWriter writer, object message);

        /// <summary>
        /// 解码。
        /// </summary>
        /// <param name="reader">读取器。</param>
        /// <param name="type">消息类型。</param>
        /// <returns>消息实例。</returns>
        object Decode(TextReader reader, Type type);
    }

    public static class CodecExtensions
    {
        public static byte[] EncodeToBytes(this ICodec codec, object message)
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            {
                codec.Encode(writer, message);
                if (!writer.AutoFlush)
                    writer.Flush();
                return memoryStream.ToArray();
            }
        }

        public static string EncodeToString(this ICodec codec, object message)
        {
            using (var writer = new StringWriter())
            {
                codec.Encode(writer, message);
                return writer.ToString();
            }
        }

        public static T DecodeByBytes<T>(this ICodec codec, byte[] data)
        {
            return (T)codec.DecodeByBytes(data, typeof(T));
        }

        public static T DecodeByString<T>(this ICodec codec, string content)
        {
            return (T)codec.DecodeByString(content, typeof(T));
        }

        public static object DecodeByBytes(this ICodec codec, byte[] data, Type type)
        {
            using (var reader = new StreamReader(new MemoryStream(data), Encoding.UTF8))
            {
                return codec.Decode(reader, type);
            }
        }

        public static object DecodeByString(this ICodec codec, string content, Type type)
        {
            using (var reader = new StringReader(content))
            {
                return codec.Decode(reader, type);
            }
        }
    }
}