using System.Threading.Tasks;

namespace Rabbit.Cloud.Abstractions
{
    public interface IRabbitMiddleware
    {
        Task InvokeAsync(RabbitContext context, RabbitRequestDelegate next);
    }
}