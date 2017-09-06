using System.Net.Http;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public class FacadeContext
    {
        public FacadeRequest Request { get; set; }
        public HttpResponseMessage Response { get; set; }
    }

    public abstract class FacadeRequest
    {
        public HttpRequestMessage RequestMessage { get; set; }
    }

    public abstract class FacadeResponse
    {
        public HttpResponseMessage ResponseMessage { get; set; }
    }
}