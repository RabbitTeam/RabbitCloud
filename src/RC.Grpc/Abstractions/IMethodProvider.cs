using Grpc.Core;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    public class MethodProviderContext
    {
        public IList<IMethod> Results { get; } = new List<IMethod>();
    }

    public interface IMethodProvider
    {
        int Order { get; }

        void OnProvidersExecuting(MethodProviderContext context);

        void OnProvidersExecuted(MethodProviderContext context);
    }
}