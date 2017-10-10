using Newtonsoft.Json;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Formatters.Json
{
    public class JsonInputFormatter : IInputFormatter
    {
        protected JsonSerializerSettings SerializerSettings { get; }
        private readonly Internal.JsonArrayPool<char> _charPool;
        private JsonSerializer _serializer;

        public JsonInputFormatter(JsonSerializerSettings jsonSerializerSettings, ArrayPool<char> charPool)
        {
            SerializerSettings = jsonSerializerSettings;
            _charPool = new Internal.JsonArrayPool<char>(charPool);
        }

        #region Implementation of IInputFormatter

        public bool CanWriteResult(InputFormatterCanWriteContext context)
        {
            return context.ContentType.Equals("application/json");
        }

        public async Task WriteAsync(InputFormatterWriteContext context)
        {
            var request = context.RabbitContext.Request;

            using (var streamWriter = new StreamWriter(request.Body))
            {
                WriteObject(streamWriter, context.Object);
                await streamWriter.FlushAsync();

                request.Headers["Content-Type"] = "application/json";
            }
        }

        private void WriteObject(TextWriter writer, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            using (var jsonWriter = CreateJsonWriter(writer))
            {
                var jsonSerializer = CreateJsonSerializer();
                jsonSerializer.Serialize(jsonWriter, value);
            }
        }

        #endregion Implementation of IInputFormatter

        private JsonWriter CreateJsonWriter(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var jsonWriter = new JsonTextWriter(writer)
            {
                ArrayPool = _charPool,
                CloseOutput = false,
                AutoCompleteOnClose = false
            };

            return jsonWriter;
        }

        private JsonSerializer CreateJsonSerializer()
        {
            return _serializer ?? (_serializer = JsonSerializer.Create(SerializerSettings));
        }
    }
}