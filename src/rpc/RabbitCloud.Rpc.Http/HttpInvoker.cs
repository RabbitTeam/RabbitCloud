using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Http
{
    public class HttpInvoker : ProtocolInvoker
    {
        private readonly ICodec _codec;
        private readonly HttpClient _httpClient;

        public HttpInvoker(Url url, ICodec codec, HttpClient httpClient) : base(url)
        {
            _codec = codec;
            _httpClient = httpClient;
        }

        #region Overrides of ProtocolInvoker

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="invocation">调用信息。</param>
        /// <returns>返回结果。</returns>
        protected override async Task<IResult> DoInvoke(IInvocation invocation)
        {
            var data = _codec.EncodeToBytes(invocation);

            var client = _httpClient;
            HttpContent content = new ByteArrayContent(data);
            var message = await client.PostAsync(new Uri(Url.ToString(), UriKind.Absolute), content);
            var bytes = await message.Content.ReadAsByteArrayAsync();
            return _codec.DecodeByBytes<RpcResult>(bytes);
        }

        #endregion Overrides of ProtocolInvoker
    }
}