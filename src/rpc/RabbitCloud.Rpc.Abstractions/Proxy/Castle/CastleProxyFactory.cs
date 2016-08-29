using Castle.DynamicProxy;
using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions.Proxy.Castle
{
    public class CastleProxyFactory : ProxyFactory
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        #region Overrides of ProxyFactory

        public override IInvoker GetInvoker(Func<object> getInstance, Id id)
        {
            return new ClrProxyInvoker(id, getInstance);
        }

        public override T GetProxy<T>(IInvoker invoker, Type[] types)
        {
            var instance = ProxyGenerator.CreateInterfaceProxyWithoutTarget(typeof(T), types, new InvokerInterceptor(invoker));
            return (T)instance;
        }

        #endregion Overrides of ProxyFactory
    }
}