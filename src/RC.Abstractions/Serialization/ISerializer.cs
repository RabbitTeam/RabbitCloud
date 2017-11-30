using System;
using System.IO;
using System.Linq;

namespace Rabbit.Cloud.Abstractions.Serialization
{
    public interface ISerializer
    {
        void Serialize(Stream stream, object instance);

        object Deserialize(Type type, Stream stream);
    }

    public static class SerializerExtensions
    {
        public static byte[] Serialize(this ISerializer serializer, object instance)
        {
            if (instance == null)
                return null;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, instance);
                return stream.ToArray();
            }
        }

        public static object Deserialize(this ISerializer serializer, Type type, byte[] data)
        {
            if (data == null || !data.Any())
                return null;

            using (var stream = new MemoryStream(data))
            {
                return serializer.Deserialize(type, stream);
            }
        }
    }
}