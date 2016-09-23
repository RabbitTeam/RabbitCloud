using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Http
{
    public class HttpInvoker : IInvoker
    {
        private readonly ICodec _codec;
        private readonly HttpClient _httpClient;

        public HttpInvoker(Url url, ICodec codec, HttpClient httpClient)
        {
            _codec = codec;
            _httpClient = httpClient;
            Url = url;
        }

        #region Implementation of IInvoker

        public Url Url { get; }

        public async Task<IResult> Invoke(IInvocation invocation)
        {
            var data = _codec.EncodeToBytes(invocation);

            var client = _httpClient;
            HttpContent content = new ByteArrayContent(data);
            var message = await client.PostAsync(new Uri(Url.ToString(), UriKind.Absolute), content);
            var bytes = await message.Content.ReadAsByteArrayAsync();
            return _codec.DecodeByBytes<Result>(bytes);
        }

        #endregion Implementation of IInvoker
    }
}