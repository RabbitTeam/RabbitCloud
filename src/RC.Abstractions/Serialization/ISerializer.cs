using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rabbit.Cloud.Abstractions.Serialization
{
    public interface ISerializer
    {
        bool CanHandle(Type type);

        void Serialize(Stream stream, object instance);

        object Deserialize(Type type, Stream stream);
    }

    public static class SerializerExtensions
    {
        public static ISerializer FindAvailableSerializer(this IEnumerable<ISerializer> serializers, Type type)
        {
            var serializer = serializers.FirstOrDefault(i => i.CanHandle(type));
            return serializer;
        }

        public static byte[] Serialize(this ISerializer serializer, object instance)
        {
            if (instance == null || !serializer.CanHandle(instance.GetType()))
                return null;

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, instance);
                return stream.ToArray();
            }
        }

        public static object Deserialize(this ISerializer serializer, Type type, byte[] data)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(data));
            if (data == null || !serializer.CanHandle(type))
                return null;

            using (var stream = new MemoryStream(data))
            {
                return serializer.Deserialize(type, stream);
            }
        }
    }
}