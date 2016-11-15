using RabbitCloud.Rpc.Abstractions.Features;
using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Hosting.Server
{
    /// <summary>
    /// 一个抽象的Rpc应用程序。
    /// </summary>
    /// <typeparam name="TContext">上下文类型。</typeparam>
    public interface IRpcApplication<TContext>
    {
        /// <summary>
        /// 创建一个上下文。
        /// </summary>
        /// <param name="contextFeatures">上下文特性。</param>
        /// <returns>上下文实例。</returns>
        TContext CreateContext(IRpcFeatureCollection contextFeatures);

        /// <summary>
        /// 处理一个请求。
        /// </summary>
        /// <param name="context">上下文实例。</param>
        /// <returns>处理请求的任务。</returns>
        Task ProcessRequestAsync(TContext context);

        /// <summary>
        /// 释放一个上下文。
        /// </summary>
        /// <param name="context">上下文实例。</param>
        /// <param name="exception">未释放完成需要抛出的异常。</param>
        void DisposeContext(TContext context, Exception exception);
    }

    public class RpcApplication : IRpcApplication<RpcApplication.Context>
    {
        private readonly RpcRequestDelegate _applicationRequest;

        public RpcApplication(RpcRequestDelegate applicationRequest)
        {
            _applicationRequest = applicationRequest;
        }

        #region Implementation of IRpcApplication<Context>

        /// <summary>
        /// 创建一个上下文。
        /// </summary>
        /// <param name="contextFeatures">上下文特性。</param>
        /// <returns>上下文实例。</returns>
        public Context CreateContext(IRpcFeatureCollection contextFeatures)
        {
            return new Context
            {
                RpcContext = new DefaultRpcContext(contextFeatures)
            };
        }

        /// <summary>
        /// 处理一个请求。
        /// </summary>
        /// <param name="context">上下文实例。</param>
        /// <returns>处理请求的任务。</returns>
        public Task ProcessRequestAsync(Context context)
        {
            return _applicationRequest(context.RpcContext);
        }

        /// <summary>
        /// 释放一个上下文。
        /// </summary>
        /// <param name="context">上下文实例。</param>
        /// <param name="exception">未释放完成需要抛出的异常。</param>
        public void DisposeContext(Context context, Exception exception)
        {
        }

        #endregion Implementation of IRpcApplication<Context>

        #region Help Class

        public struct Context
        {
            public RpcContext RpcContext { get; set; }
        }

        #endregion Help Class
    }
}