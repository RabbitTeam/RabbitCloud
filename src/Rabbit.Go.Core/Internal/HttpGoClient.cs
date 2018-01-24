using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Go.Internal
{
    public class HttpGoClient : IGoClient
    {
        public static IGoClient Instance { get; } = new HttpGoClient();
        private readonly HttpClient _httpClient = new HttpClient();

        #region Implementation of IGoClient

        public async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage request, RequestOptions options)
        {
            using (var tokenSource = new CancellationTokenSource(options.Timeout))
            {
                var response = await _httpClient.SendAsync(request, tokenSource.Token);
                return response;
            }
        }

        #endregion Implementation of IGoClient
    }
}