using System;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Abstractions.Serialization
{
    public interface ISerializer
    {
        bool Serialize(Stream stream, object instance);

        object Deserialize(Type type, Stream stream);
    }

    public static class SerializerExtensions
    {
        public static byte[] Serialize(this IEnumerable<ISerializer> serializers, object instance)
        {
            if (instance == null)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                foreach (var serializer in serializers)
                {
                    if (serializer.Serialize(memoryStream, instance))
                        return memoryStream.ToArray();
                }
                return null;
            }
        }

        public static object Deserialize(this IEnumerable<ISerializer> serializers, Type type, byte[] data)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(data));
            if (data == null)
                return null;

            using (var memoryStream = new MemoryStream(data))
            {
                foreach (var serializer in serializers)
                {
                    var value = serializer.Deserialize(type, memoryStream);
                    if (value != null)
                        return value;
                }
                return null;
            }
        }

        public static byte[] Serialize(this ISerializer serializer, object instance)
        {
            if (instance == null)
                return null;

            using (var stream = new MemoryStream())
            {
                return serializer.Serialize(stream, instance) ? stream.ToArray() : null;
            }
        }

        public static object Deserialize(this ISerializer serializer, Type type, byte[] data)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(data));
            if (data == null)
                return null;

            using (var stream = new MemoryStream(data))
            {
                return serializer.Deserialize(type, stream);
            }
        }
    }
}