using Rabbit.Cloud;
using System.Threading.Tasks;

namespace Samples.Service
{
    public class Request
    {
        public string Name { get; set; }
    }

    public class Response
    {
        public string Message { get; set; }
    }

    [RabbitClient("grpc://Samples.Service/TestService")]
    public interface ITestService
    {
        Task<Response> SendAsync(Request request);

        Task<Response> Send2Async(string name, int age);
    }

    [RabbitService("TestService")]
    public class TestService : ITestService
    {
        #region Implementation of IServiceBase

        public Task<Response> SendAsync(Request request)
        {
            return Task.FromResult(new Response
            {
                Message = "hello " + request.Name
            });
        }

        public Task<Response> Send2Async(string name, int age)
        {
            return Task.FromResult(new Response
            {
                Message = "hello " + name + ",age " + age
            });
        }

        #endregion Implementation of IServiceBase
    }
}