using System;
using System.IO;

namespace Rabbit.Cloud.Client.Serialization
{
    public interface ISerializer
    {
        void Serialize(object instance, Stream stream);

        object Deserialize(Stream stream, Type type);
    }

    public static class SerializerExtensions
    {
        public static byte[] Serializer(this ISerializer serializer, object instance)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(instance, stream);
                return stream.ToArray();
            }
        }

        public static object Deserialize(this ISerializer serializer, byte[] data, Type type)
        {
            using (var stream = new MemoryStream(data))
            {
                return serializer.Deserialize(stream, type);
            }
        }
    }
}