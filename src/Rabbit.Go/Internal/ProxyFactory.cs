using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Go.Internal
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly IInterceptor[] _interceptors;
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public ProxyFactory(IEnumerable<IInterceptor> interceptors)
        {
            _interceptors = interceptors.ToArray();
        }

        #region Implementation of IProxyFactory

        public object CreateProxy(Type interfaceType)
        {
            return ProxyGenerator.CreateInterfaceProxyWithoutTarget(interfaceType, new Type[0], _interceptors);
        }

        #endregion Implementation of IProxyFactory
    }
    public interface IProxyFactory
    {
        object CreateProxy(Type interfaceType);
    }

    public static class ProxyFactoryExtensions
    {
        public static T CreateProxy<T>(this IProxyFactory proxyFactory)
        {
            return (T)proxyFactory.CreateProxy(typeof(T));
        }
    }
}