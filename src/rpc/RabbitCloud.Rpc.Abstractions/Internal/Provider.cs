using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions.Utils;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Internal
{
    /// <summary>
    /// RPC提供程序抽象类。
    /// </summary>
    public abstract class Provider : IProvider
    {
        protected readonly IDictionary<string, MethodInfo> MethodDictionary = new Dictionary<string, MethodInfo>();

        protected Provider(Url url, Type type)
        {
            InterfaceType = type;
            Url = url;

            foreach (var methodInfo in type.GetMethods())
            {
                var signature = ReflectUtil.GetMethodSignature(methodInfo);
                MethodDictionary[signature] = methodInfo;
            }
        }

        #region Implementation of INode

        /// <summary>
        /// 节点Url。
        /// </summary>
        public Url Url { get; }

        /// <summary>
        /// 是否可用。
        /// </summary>
        public bool IsAvailable { get; private set; } = true;

        /// <summary>
        /// 接口类型。
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// 调用RPC请求。
        /// </summary>
        /// <param name="request">调用请求。</param>
        /// <returns>RPC请求响应结果。</returns>
        public async Task<IResponse> Call(IRequest request)
        {
            var response = await Invoke(request);
            return response;
        }

        #endregion Implementation of INode

        #region Implementation of IDisposable

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            IsAvailable = false;
        }

        #endregion Implementation of IDisposable

        #region Protected Method

        /// <summary>
        /// 针对RPC请求执行调用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>RPC响应结果。</returns>
        protected abstract Task<IResponse> Invoke(IRequest request);

        /// <summary>
        /// 根据RPC请求获取方法信息。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>方法信息。</returns>
        protected MethodInfo GetMethod(IRequest request)
        {
            return MethodDictionary[request.GetMethodSignature()];
        }

        #endregion Protected Method
    }
}