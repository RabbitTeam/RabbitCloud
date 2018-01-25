using System;

namespace Rabbit.Go.Interceptors
{
    public interface IInterceptorFactory : IInterceptorMetadata
    {
        bool IsReusable { get; }

        IInterceptorMetadata CreateInstance(IServiceProvider serviceProvider);
    }
}