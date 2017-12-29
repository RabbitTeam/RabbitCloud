namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRequestInvokerProvider
    {
        int Order { get; }

        void OnProvidersExecuting(RequestInvokerProviderContext context);

        void OnProvidersExecuted(RequestInvokerProviderContext context);
    }
}