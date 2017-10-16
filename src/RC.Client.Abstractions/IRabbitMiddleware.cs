using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitMiddleware
    {
        Task InvokeAsync(IRabbitContext context, RabbitRequestDelegate next);
    }
}