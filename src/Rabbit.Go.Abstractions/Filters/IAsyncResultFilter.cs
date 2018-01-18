using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions.Filters
{
    public interface IAsyncResultFilter : IFilterMetadata
    {
        Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next);
    }
}