using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rabbit.Go.Core.Internal
{
    public class DefaultMethodInvokerFactory : IMethodInvokerFactory
    {
        private readonly MethodInvokerCache _methodInvokerCache;
        private readonly IServiceProvider _services;

        public DefaultMethodInvokerFactory(MethodInvokerCache methodInvokerCache, IServiceProvider services)
        {
            _methodInvokerCache = methodInvokerCache;
            _services = services;
        }

        public IMethodInvoker CreateInvoker(MethodDescriptor methodDescriptor)
        {
            var entry = _methodInvokerCache.Get(methodDescriptor);

            var requestServices = _services.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider;

            var goContext = new GoContext
            {
                RequestServices = requestServices
            };

            return new DefaultMethodInvoker(new RequestContext(goContext, methodDescriptor), entry);
        }
    }
}