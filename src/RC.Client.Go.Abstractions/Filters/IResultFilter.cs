namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public interface IResultFilter : IFilterMetadata
    {
        void OnResultExecuting(ResultExecutingContext context);

        void OnResultExecuted(ResultExecutedContext context);
    }
}