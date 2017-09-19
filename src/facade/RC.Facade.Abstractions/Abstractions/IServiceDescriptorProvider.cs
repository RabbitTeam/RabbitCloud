namespace Rabbit.Cloud.Facade.Abstractions.Abstractions
{
    public interface IServiceDescriptorProvider
    {
        int Order { get; }

        void OnProvidersExecuting(ServiceDescriptorProviderContext context);

        void OnProvidersExecuted(ServiceDescriptorProviderContext context);
    }
}