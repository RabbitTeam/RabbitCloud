using System;
using System.IO;
using System.Text;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface ICodec
    {
        void Encode(TextWriter writer, object message);

        object Decode(TextReader reader, Type type);
    }

    public static class CodecExtensions
    {
        public static byte[] EncodeToBytes(this ICodec codec, object message)
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.Unicode))
            {
                codec.Encode(writer, message);
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
            using (var reader = new StreamReader(new MemoryStream(data)))
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