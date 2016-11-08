using RabbitCloud.Abstractions;
using System;
using System.Linq;
using System.Reflection;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的调用。
    /// </summary>
    public interface IInvocation
    {
        /// <summary>
        /// 方法名称。
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// 参数类型。
        /// </summary>
        Type[] ParameterTypes { get; }

        /// <summary>
        /// 方法参数。
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        /// 调用者。
        /// </summary>
        IInvoker Invoker { get; }

        /// <summary>
        /// 属性。
        /// </summary>
        AttributeDictionary Attributes { get; }
    }

    /// <summary>
    /// Rpc调用。
    /// </summary>
    public class RpcInvocation : IInvocation
    {
        public RpcInvocation()
        {
            Attributes = new AttributeDictionary();
        }

        #region Implementation of IInvocation

        /// <summary>
        /// 方法名称。
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 参数类型。
        /// </summary>
        public Type[] ParameterTypes { get; set; }

        /// <summary>
        /// 方法参数。
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// 调用者。
        /// </summary>
        public IInvoker Invoker { get; set; }

        /// <summary>
        /// 属性。
        /// </summary>
        public AttributeDictionary Attributes { get; set; }

        #endregion Implementation of IInvocation

        #region Public Static Method

        public static RpcInvocation Create(MethodInfo method, object[] arguments, IInvoker invoker = null)
        {
            return new RpcInvocation
            {
                Arguments = arguments,
                MethodName = method.Name,
                ParameterTypes = method.GetParameters().Select(i => i.ParameterType).ToArray(),
                Invoker = invoker,
                Attributes = new AttributeDictionary()
            };
        }

        #endregion Public Static Method
    }
}