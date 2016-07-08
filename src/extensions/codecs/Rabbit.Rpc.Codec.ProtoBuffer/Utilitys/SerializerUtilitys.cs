using ProtoBuf;
using System;
using System.IO;

namespace Rabbit.Rpc.Codec.ProtoBuffer.Utilitys
{
    public static class SerializerUtilitys
    {
        public static byte[] Serialize(object instance)
        {
            using (var stream = new MemoryStream())
            {
#if NET
                Serializer.NonGeneric.Serialize(stream, instance);
#else
                Serializer.Serialize(stream,instance);
#endif
                return stream.ToArray();
            }
        }

        public static object Deserialize(byte[] data, Type type)
        {
            if (data == null)
                return null;
            using (var stream = new MemoryStream(data))
            {
#if NET
                return Serializer.NonGeneric.Deserialize(type, stream);
#else
                return Serializer.Deserialize(type, stream);
#endif
            }
        }

        public static T Deserialize<T>(byte[] data)
        {
            if (data == null)
                return default(T);
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }
    }
}