using Newtonsoft.Json;
using System;
using System.IO;

namespace Rabbit.Cloud.Serialization.Json
{
    public class JsonSerializer : Serializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _jsonSerializer;

        public JsonSerializer()
        {
            _jsonSerializer = Newtonsoft.Json.JsonSerializer.CreateDefault();
        }

        public JsonSerializer(Newtonsoft.Json.JsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public JsonSerializer(JsonSerializerSettings settings)
        {
            _jsonSerializer = Newtonsoft.Json.JsonSerializer.Create(settings);
        }

        #region Overrides of Serializer

        protected override bool CanHandle(Type type)
        {
            return true;
        }

        protected override void DoSerialize(Stream stream, object instance)
        {
            using (TextWriter writer = new StreamWriter(stream))
                _jsonSerializer.Serialize(writer, instance);
        }

        protected override object DoDeserialize(Type type, Stream stream)
        {
            using (TextReader reader = new StreamReader(stream))
            {
                return _jsonSerializer.Deserialize(reader, type);
            }
        }

        #endregion Overrides of Serializer
    }
}