using Newtonsoft.Json;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Codec;
using Rabbit.Go.Codec;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Go.Core.Codec
{
    public class JsonCodec : ICodec
    {
        public static ICodec Instance { get; } = new JsonCodec();

        public JsonCodec() : this(JsonSerializer.CreateDefault())
        {
        }

        public JsonCodec(JsonSerializer jsonSerializer)
        {
            Encoder = new JsonEncoder(jsonSerializer);
            Decoder = new JsonDecoder(jsonSerializer);
        }

        #region Implementation of ICodec

        public IEncoder Encoder { get; }
        public IDecoder Decoder { get; }

        #endregion Implementation of ICodec

        public class JsonEncoder : IEncoder
        {
            private readonly JsonSerializer _jsonSerializer;

            public JsonEncoder(JsonSerializer jsonSerializer)
            {
                _jsonSerializer = jsonSerializer;
            }

            #region Implementation of IEncoder

            public Task EncodeAsync(object instance, Type type, GoRequest request)
            {
                if (instance == null)
                    return Task.CompletedTask;

                using (var sw = new StringWriter())
                {
                    _jsonSerializer.Serialize(sw, instance, type);

                    request.Body(sw.ToString());
                }

                return Task.CompletedTask;
            }

            #endregion Implementation of IEncoder
        }

        public class JsonDecoder : IDecoder
        {
            private readonly JsonSerializer _jsonSerializer;

            public JsonDecoder(JsonSerializer jsonSerializer)
            {
                _jsonSerializer = jsonSerializer;
            }

            #region Implementation of IDecoder

            public async Task<object> DecodeAsync(GoResponse response, Type type)
            {
                if (response.Content == null)
                    return null;

                string json;
                using (var streamReader = new StreamReader(response.Content, Encoding.UTF8))
                    json = await streamReader.ReadToEndAsync();

                using (var sr = new StringReader(json))
                {
                    return _jsonSerializer.Deserialize(sr, type);
                }
            }

            #endregion Implementation of IDecoder
        }
    }
}