using System.Threading.Tasks;

namespace RC.Abstractions
{
    public interface IRabbitMiddleware
    {
        Task InvokeAsync(RabbitContext context, RabbitRequestDelegate next);
    }
}