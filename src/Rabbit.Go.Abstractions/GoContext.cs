using System.Net.Http;

namespace Rabbit.Go.Abstractions
{
    public class GoContext
    {
        public HttpRequestMessage RequestMessage { get; set; }
        public HttpResponseMessage ResponseMessage { get; set; }
    }
}