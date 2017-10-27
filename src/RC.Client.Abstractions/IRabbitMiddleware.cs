using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitMiddleware<TContext>
    {
        Task InvokeAsync(TContext context, RabbitRequestDelegate<TContext> next);
    }
}