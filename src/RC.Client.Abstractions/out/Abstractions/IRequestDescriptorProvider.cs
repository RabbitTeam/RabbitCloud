namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRequestDescriptorProvider
    {
        int Order { get; }

        void OnProvidersExecuting(RequestDescriptorProviderContext context);

        void OnProvidersExecuted(RequestDescriptorProviderContext context);
    }
}