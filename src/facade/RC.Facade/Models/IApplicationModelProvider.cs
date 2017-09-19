namespace Rabbit.Cloud.Facade.Models
{
    public interface IApplicationModelProvider
    {
        int Order { get; }

        void OnProvidersExecuting(ApplicationModelProviderContext context);

        void OnProvidersExecuted(ApplicationModelProviderContext context);
    }
}