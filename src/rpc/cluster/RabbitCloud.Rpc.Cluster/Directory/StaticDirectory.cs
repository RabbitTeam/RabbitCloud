using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Rpc.Cluster.Directory
{
    public class StaticDirectory : Directory
    {
        private IInvoker[] _invokers;

        public StaticDirectory(Url url) : base(url)
        {
        }

        public StaticDirectory(Url url, IEnumerable<IRouter> routers) : base(url, routers)
        {
        }

        public StaticDirectory(Url url, Url consumerUrl, IEnumerable<IRouter> routers) : base(url, consumerUrl, routers)
        {
        }

        public StaticDirectory(Url url, IInvoker[] invokers, IEnumerable<IRouter> routers) : base(url == null && invokers != null && invokers.Any() ? invokers.First().Url : url, routers)
        {
            _invokers = invokers;
        }

        #region Overrides of Directory

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public override void Dispose()
        {
            if (IsDisposed)
                return;
            base.Dispose();
            foreach (var invoker in _invokers)
            {
                invoker.Dispose();
            }
            _invokers = new IInvoker[0];
        }

        /// <summary>
        /// 是否可用。
        /// </summary>
        public override bool IsAvailable => !IsDisposed && _invokers.Any(i => i.IsAvailable);

        protected override IInvoker[] DoGetInvokers(IInvocation invocation)
        {
            return _invokers;
        }

        #endregion Overrides of Directory
    }
}