namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public interface IApplicationModelProvider
    {
        int Order { get; }

        void OnProvidersExecuting(ApplicationModelProviderContext context);

        void OnProvidersExecuted(ApplicationModelProviderContext context);
    }
}