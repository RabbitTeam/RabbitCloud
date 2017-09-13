namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public interface IResultFilter : IFilterMetadata
    {
        void OnResultExecuting(ResultExecutingContext context);

        void OnResultExecuted(ResultExecutedContext context);
    }
}