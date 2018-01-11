using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Go.Abstractions;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go
{
    public class ServiceInvokerContext
    {
        public GoRequestContext RequestContext { get; set; }
        public RequestModel RequestModel { get; set; }
        public RabbitRequestDelegate Invoker { get; set; }
    }

    public interface IServiceInvoker
    {
        Task InvokeAsync();
    }
}