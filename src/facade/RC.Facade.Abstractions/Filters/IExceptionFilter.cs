namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public interface IExceptionFilter : IFilterMetadata
    {
        void OnException(ExceptionContext context);
    }
}