namespace Rabbit.Go.Core.GoModels
{
    public interface IGoModelProvider
    {
        int Order { get; }

        void OnProvidersExecuting(GoModelProviderContext context);

        void OnProvidersExecuted(GoModelProviderContext context);
    }
}