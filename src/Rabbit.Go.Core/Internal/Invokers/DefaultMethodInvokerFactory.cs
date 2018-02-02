namespace Rabbit.Go.Core.Internal
{
    public class DefaultMethodInvokerFactory : IMethodInvokerFactory
    {
        private readonly MethodInvokerCache _methodInvokerCache;

        public DefaultMethodInvokerFactory(MethodInvokerCache methodInvokerCache)
        {
            _methodInvokerCache = methodInvokerCache;
        }

        public IMethodInvoker CreateInvoker(MethodDescriptor methodDescriptor)
        {
            var entry = _methodInvokerCache.Get(methodDescriptor);

            var goContext = new GoContext();

            return new DefaultMethodInvoker(new RequestContext(goContext, methodDescriptor), entry);
        }
    }
}