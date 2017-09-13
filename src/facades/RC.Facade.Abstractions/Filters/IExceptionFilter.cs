namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public interface IExceptionFilter
    {
        void OnException(ExceptionContext context);
    }
}