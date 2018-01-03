using System;
using System.IO;

namespace Rabbit.Cloud.Client.Serialization
{
    public class JsonSerializer : ISerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _jsonSerializer;

        public JsonSerializer()
        {
            _jsonSerializer = new Newtonsoft.Json.JsonSerializer();
        }

        #region Implementation of ISerializer

        public void Serialize(object instance, Stream stream)
        {
            var writer = new StreamWriter(stream);
            _jsonSerializer.Serialize(writer, instance);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
        }

        public object Deserialize(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream))
                return _jsonSerializer.Deserialize(reader, type);
        }

        #endregion Implementation of ISerializer
    }
}