using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitMiddleware
    {
        Task InvokeAsync(RabbitContext context, RabbitRequestDelegate next);
    }
}