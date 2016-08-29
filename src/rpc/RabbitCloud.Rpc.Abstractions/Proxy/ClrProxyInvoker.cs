using RabbitCloud.Abstractions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    public class ClrProxyInvoker : ProxyInvoker
    {
        public ClrProxyInvoker(Id id, Func<object> getInstance) : base(id, getInstance)
        {
        }

        #region Overrides of ProxyInvoker

        protected override Task<object> DoInvoke(object proxy, string methodName, Type[] parameterTypes, object[] arguments)
        {
            var method = proxy.GetType().GetRuntimeMethod(methodName, parameterTypes ?? new Type[0]);
            var result = method.Invoke(proxy, arguments);
            return Task.FromResult(result);
        }

        #endregion Overrides of ProxyInvoker
    }
}