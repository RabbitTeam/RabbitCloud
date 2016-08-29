using RabbitCloud.Abstractions.Feature;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IInvocation : IMetadataFeature
    {
        string MethodName { get; }
        Type[] ParameterTypes { get; }
        object[] Arguments { get; }
    }

    public class Invocation : IInvocation
    {
        public static Invocation Create(MethodInfo method, object[] arguments)
        {
            return new Invocation
            {
                Arguments = arguments,
                MethodName = method.Name,
                ParameterTypes = method.GetParameters().Select(i => i.ParameterType).ToArray()
            };
        }

        #region Implementation of IMetadataFeature

        /// <summary>
        /// 元数据。
        /// </summary>
        public IDictionary<string, object> Metadata { get; } = new ConcurrentDictionary<string, object>();

        #endregion Implementation of IMetadataFeature

        #region Implementation of IInvocation

        public string MethodName { get; set; }
        public Type[] ParameterTypes { get; set; }
        public object[] Arguments { get; set; }

        #endregion Implementation of IInvocation
    }
}