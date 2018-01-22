using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions
{
    public class RequestOptions
    {
        public static RequestOptions Default { get; } = new RequestOptions();

        public RequestOptions()
        {
            Timeout = TimeSpan.FromSeconds(60);
        }

        public TimeSpan Timeout { get; set; }
    }

    public interface IGoClient
    {
        Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage request, RequestOptions options);
    }
}