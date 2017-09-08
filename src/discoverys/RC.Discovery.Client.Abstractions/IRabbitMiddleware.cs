using System.Threading.Tasks;

namespace RC.Discovery.Client.Abstractions
{
    public interface IRabbitMiddleware
    {
        Task InvokeAsync(RabbitContext context, RabbitRequestDelegate next);
    }
}