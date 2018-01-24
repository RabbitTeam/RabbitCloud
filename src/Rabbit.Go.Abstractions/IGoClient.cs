using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Go
{
    public interface IGoClient
    {
        Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage request, RequestOptions options);
    }
}