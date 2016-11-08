using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Rpc.Cluster.Directory
{
    public abstract class Directory : IDirectory
    {
        #region Property

        public IRouter[] Routes { get; private set; }
        public Url ConsumerUrl { get; set; }
        public bool IsDisposed { get; private set; }

        #endregion Property

        protected Directory(Url url) : this(url, null)
        {
        }

        protected Directory(Url url, IEnumerable<IRouter> routers) : this(url, url, routers)
        {
        }

        protected Directory(Url url, Url consumerUrl, IEnumerable<IRouter> routers)
        {
            Url = url;
            ConsumerUrl = consumerUrl;
            SetRoutes(routers);
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public virtual void Dispose()
        {
            IsDisposed = true;
        }

        #endregion Implementation of IDisposable

        #region Implementation of INode

        /// <summary>
        /// 节点Url。
        /// </summary>
        public Url Url { get; }

        /// <summary>
        /// 是否可用。
        /// </summary>
        public abstract bool IsAvailable { get; }

        #endregion Implementation of INode

        #region Implementation of IDirectory

        /// <summary>
        /// 接口类型。
        /// </summary>
        public Type InterfaceType { get; set; }

        /// <summary>
        /// 获取所有调用者。
        /// </summary>
        /// <param name="invocation">调用信息。</param>
        /// <returns>该目录下所有的调用者。</returns>
        public IInvoker[] GetInvokers(IInvocation invocation)
        {
            var invokers = DoGetInvokers(invocation);
            if (Routes != null && Routes.Any())
            {
                foreach (var router in Routes)
                {
                    invokers = router.Route(invokers, ConsumerUrl, invocation);
                }
            }
            return invokers;
        }

        #endregion Implementation of IDirectory

        #region Protected Method

        protected void SetRoutes(IEnumerable<IRouter> routers)
        {
            Routes = routers == null ? new IRouter[0] : routers.ToArray();
        }

        protected abstract IInvoker[] DoGetInvokers(IInvocation invocation);

        #endregion Protected Method
    }
}