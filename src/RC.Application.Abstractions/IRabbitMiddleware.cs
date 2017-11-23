using System.Threading.Tasks;

namespace Rabbit.Cloud.Application.Abstractions
{
    public interface IRabbitMiddleware
    {
        Task InvokeAsync(IRabbitContext context, RabbitRequestDelegate next);
    }
}