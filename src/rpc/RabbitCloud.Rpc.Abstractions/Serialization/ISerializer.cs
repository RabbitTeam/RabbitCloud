using System;
using System.IO;
using System.Text;

namespace RabbitCloud.Rpc.Abstractions.Serialization
{
    public interface ISerializer
    {
        void Serialize(TextWriter writer, object value);

        object Deserialize(TextReader reader, Type type);
    }

    public static class SerializerExtensions
    {
        public static byte[] SerializeToBytes(this ISerializer codec, object value)
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.Unicode))
            {
                codec.Serialize(writer, value);
                return memoryStream.ToArray();
            }
        }

        public static string SerializeToString(this ISerializer codec, object value)
        {
            using (var writer = new StringWriter())
            {
                codec.Serialize(writer, value);
                return writer.ToString();
            }
        }

        public static object DeserializeByString(this ISerializer codec, string content, Type type)
        {
            using (var reader = new StringReader(content))
            {
                return codec.Deserialize(reader, type);
            }
        }

        public static T DeserializeByString<T>(this ISerializer codec, string content)
        {
            return (T)codec.DeserializeByString(content, typeof(T));
        }

        public static object DeserializeByBytes(this ISerializer codec, byte[] data, Type type)
        {
            using (var reader = new StreamReader(new MemoryStream(data)))
            {
                return codec.Deserialize(reader, type);
            }
        }

        public static T DeserializeByBytes<T>(this ISerializer codec, byte[] data)
        {
            return (T)codec.DeserializeByBytes(data, typeof(T));
        }
    }
}