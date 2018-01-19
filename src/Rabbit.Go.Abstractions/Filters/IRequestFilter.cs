namespace Rabbit.Go.Abstractions.Filters
{
    public interface IRequestFilter : IFilterMetadata
    {
        void OnRequestExecuting(RequestExecutingContext context);

        void OnRequestExecuted(RequestExecutedContext context);
    }
}