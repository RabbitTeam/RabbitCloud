using System;
using System.IO;

namespace RabbitCloud.Rpc.Abstractions.Serialization.Implementation
{
    public class JsonSerializer : ISerializer
    {
        private static readonly Newtonsoft.Json.JsonSerializer NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer.CreateDefault();

        #region Implementation of ISerializer

        public void Serialize(TextWriter writer, object value)
        {
            NewtonsoftJsonSerializer.Serialize(writer, value);
        }

        public object Deserialize(TextReader reader, Type type)
        {
            return NewtonsoftJsonSerializer.Deserialize(reader, type);
        }

        #endregion Implementation of ISerializer
    }
}