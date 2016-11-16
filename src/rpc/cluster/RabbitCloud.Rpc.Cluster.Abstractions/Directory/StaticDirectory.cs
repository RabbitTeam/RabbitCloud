using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions.Directory
{
    /// <summary>
    /// 一个静态的调用者目录。
    /// </summary>
    public class StaticDirectory : Directory
    {
        private readonly ICollection<ICaller> _callers;

        public StaticDirectory(Url url, ICollection<ICaller> callers) : base(url)
        {
            _callers = callers;
        }

        #region Overrides of Directory

        /// <summary>
        /// 是否可用。
        /// </summary>
        public override bool IsAvailable => !IsDisposable && _callers.Any(i => i.IsAvailable);

        /// <summary>
        /// 服务接口类型。
        /// </summary>
        public override Type InterfaceType => _callers.FirstOrDefault()?.InterfaceType;

        /// <summary>
        /// 根据RPC请求获取该服务所有的调用者。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者集合。</returns>
        protected override Task<IEnumerable<ICaller>> DoGetCallers(IRequest request)
        {
            return Task.FromResult<IEnumerable<ICaller>>(_callers);
        }

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public override void Dispose()
        {
            if (IsDisposable)
                return;
            base.Dispose();
            foreach (var caller in _callers)
            {
                caller.Dispose();
            }
            _callers.Clear();
        }

        #endregion Overrides of Directory
    }
}