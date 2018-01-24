using Newtonsoft.Json;
using System;
using System.Linq;
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
            if (response?.Content == null)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return json == null ? null : (Task<object>)JsonConvert.DeserializeObject(json, type);
        }

        #endregion Implementation of IDecoder
    }

    public class JsonEncoder : IEncoder
    {
        #region Implementation of IEncoder

        public Task EncodeAsync(object instance, Type type, RequestContext requestContext)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (instance == null)
                return Task.CompletedTask;

            var json = JsonConvert.SerializeObject(instance);
            requestContext.Body = string.IsNullOrEmpty(json) ? Enumerable.Empty<byte>().ToArray() : Encoding.UTF8.GetBytes(json);
            return Task.CompletedTask;
        }

        #endregion Implementation of IEncoder
    }
}