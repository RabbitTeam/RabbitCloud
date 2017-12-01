using Rabbit.Cloud.Abstractions.Serialization;
using System.Collections.Generic;

namespace Rabbit.Cloud.Serialization.Json
{
    public static class SerializersExtensions
    {
        public static ICollection<ISerializer> AddJsonSerializer(this ICollection<ISerializer> serializers)
        {
            serializers.Add(new JsonSerializer());
            return serializers;
        }
    }
}