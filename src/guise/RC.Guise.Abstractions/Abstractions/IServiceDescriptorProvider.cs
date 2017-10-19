namespace Rabbit.Cloud.Guise.Abstractions
{
    public interface IServiceDescriptorProvider
    {
        int Order { get; }

        void OnProvidersExecuting(ServiceDescriptorProviderContext context);

        void OnProvidersExecuted(ServiceDescriptorProviderContext context);
    }
}