using RabbitCloud.Abstractions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    public class ClrProxyInvoker : ProxyInvoker
    {
        public ClrProxyInvoker(Url url, Func<object> getInstance) : base(url, getInstance)
        {
        }

        #region Overrides of ProxyInvoker

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="proxy">代理实例。</param>
        /// <param name="methodName">方法名称。</param>
        /// <param name="parameterTypes">参数类型。</param>
        /// <param name="arguments">方法参数。</param>
        /// <returns>返回值。</returns>
        protected override Task<object> DoInvoke(object proxy, string methodName, Type[] parameterTypes, object[] arguments)
        {
            var method = proxy.GetType().GetRuntimeMethod(methodName, parameterTypes ?? new Type[0]);
            var result = method.Invoke(proxy, arguments);
            return Task.FromResult(result);
        }

        #endregion Overrides of ProxyInvoker
    }
}