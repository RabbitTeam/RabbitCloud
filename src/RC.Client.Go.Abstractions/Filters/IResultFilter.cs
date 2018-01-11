namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public interface IResultFilter : IFilterMetadata
    {
        void OnResultExecuted(ResultExecutedContext context);

        void OnResultExecuting(ResultExecutingContext context);
    }
}