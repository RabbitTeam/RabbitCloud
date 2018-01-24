using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Go.Codec
{
    public class JsonDecoder : IDecoder
    {
        #region Implementation of IDecoder

        public async Task<object> DecodeAsync(HttpResponseMessage response, Type type)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(json, type);
        }

        #endregion Implementation of IDecoder
    }

    public class JsonEncoder : IEncoder
    {
        #region Implementation of IEncoder

        public Task EncodeAsync(object instance, Type type, RequestContext requestContext)
        {
            var json = JsonConvert.SerializeObject(instance);

            requestContext.Body = Encoding.UTF8.GetBytes(json);
            return Task.CompletedTask;
        }

        #endregion Implementation of IEncoder
    }
}