using Rabbit.Cloud.Abstractions.Serialization;
using System.Collections.Generic;

namespace Rabbit.Cloud.Serialization.MessagePack
{
    public static class SerializersExtensions
    {
        public static ICollection<ISerializer> AddMessagePackSerializer(this ICollection<ISerializer> serializers)
        {
            serializers.Add(new MessagePackSerializer());
            return serializers;
        }
    }
}