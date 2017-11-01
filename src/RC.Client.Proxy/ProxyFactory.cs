using Castle.DynamicProxy;
using System;

namespace Rabbit.Cloud.Client.Proxy
{
    public class ProxyFactory
    {
        private readonly IInterceptor[] _interceptors;
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public ProxyFactory(params IInterceptor[] interceptors)
        {
            _interceptors = interceptors;
        }

        public object CreateInterfaceProxy(Type interfaceType)
        {
            return ProxyGenerator.CreateInterfaceProxyWithoutTarget(interfaceType, new Type[0], _interceptors);
        }
    }
}