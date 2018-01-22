namespace Rabbit.Go.Abstractions
{
    public interface IMethodDescriptorProvider
    {
        int Order { get; }

        void OnProvidersExecuting(MethodDescriptorProviderContext context);

        void OnProvidersExecuted(MethodDescriptorProviderContext context);
    }
}