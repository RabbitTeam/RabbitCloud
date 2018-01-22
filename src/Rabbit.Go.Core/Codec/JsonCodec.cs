using Newtonsoft.Json;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Codec;
using System;
using System.Net.Http;
using System.Text;

namespace Rabbit.Go.Core.Codec
{
    public class JsonDecoder : IDecoder
    {
        #region Implementation of IDecoder

        public object Decode(HttpResponseMessage response, Type type)
        {
            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject(json, type);
        }

        #endregion Implementation of IDecoder
    }

    public class JsonEncoder : IEncoder
    {
        #region Implementation of IEncoder

        public void Encode(object instance, Type type, RequestContext requestContext)
        {
            var json = JsonConvert.SerializeObject(instance);

            requestContext.Body = Encoding.UTF8.GetBytes(json);
        }

        #endregion Implementation of IEncoder
    }
}