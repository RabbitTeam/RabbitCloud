using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<Type, ISerializer> SerializerCaches = new ConcurrentDictionary<Type, ISerializer>();

        public static byte[] Serialize(this IEnumerable<ISerializer> serializers, object instance)
        {
            if (instance == null)
                return null;
            var type = instance.GetType();
            if (SerializerCaches.TryGetValue(type, out var serializer) && serializer != null)
                return serializer.Serialize(instance);

            using (var memoryStream = new MemoryStream())
            {
                foreach (var s in serializers)
                {
                    if (!s.Serialize(memoryStream, instance))
                        continue;
                    SerializerCaches.TryAdd(type, s);
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

            if (SerializerCaches.TryGetValue(type, out var serializer) && serializer != null)
                return serializer.Deserialize(type, data);

            using (var memoryStream = new MemoryStream(data))
            {
                foreach (var s in serializers)
                {
                    var value = s.Deserialize(type, memoryStream);
                    if (value == null)
                        continue;
                    SerializerCaches.TryAdd(type, s);
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