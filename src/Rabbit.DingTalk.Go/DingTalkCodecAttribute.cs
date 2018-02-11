using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rabbit.Go;
using Rabbit.Go.Codec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Rabbit.DingTalk.Go
{
    public class DingTalkCodecAttribute : Attribute, IEncoder, IDecoder, ICodec
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        #region Implementation of IEncoder

        public Task EncodeAsync(object instance, Type type, GoRequest request)
        {
            if (!(instance is DingTalkMessage dingTalkMessage))
                return Task.CompletedTask;

            var messageType = dingTalkMessage.Type.ToString();

            messageType = messageType[0].ToString().ToLower() + messageType.Substring(1);
            var propertyName = messageType;

            var dictionary = new
                Dictionary<string, object>
                {
                    {"msgtype",messageType },
                    {propertyName, instance}
                };

            if (instance is IAtMessage atMessage)
            {
                dictionary["at"] = new
                {
                    atMobiles = atMessage.AtMobiles,
                    isAtAll = atMessage.IsAtAll
                };
            }

            var json = JsonConvert.SerializeObject(dictionary, SerializerSettings);
            request.Body(json, "application/json");

            return Task.CompletedTask;
        }

        #endregion Implementation of IEncoder

        #region Implementation of IDecoder

        public async Task<object> DecodeAsync(GoResponse response, Type type)
        {
            using (var streamReader = new StreamReader(response.Content))
            {
                var json = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject(json, type, SerializerSettings);
            }
        }

        #endregion Implementation of IDecoder

        #region Implementation of ICodec

        public IEncoder Encoder => this;
        public IDecoder Decoder => this;

        #endregion Implementation of ICodec
    }
}