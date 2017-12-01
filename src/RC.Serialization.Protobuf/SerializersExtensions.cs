using Rabbit.Cloud.Abstractions.Serialization;
using System.Collections.Generic;

namespace Rabbit.Cloud.Serialization.Protobuf
{
    public static class SerializersExtensions
    {
        public static ICollection<ISerializer> AddProtobufSerializer(this ICollection<ISerializer> serializers)
        {
            serializers.Add(new ProtobufSerializer());
            return serializers;
        }
    }
}