using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions.HaStrategy
{
    /// <summary>
    /// 故障切换高可用策略。
    /// </summary>
    public class FailoverHaStrategy : HaStrategy
    {
        private readonly ThreadLocal<IList<IReferer>> _referersHolder = new ThreadLocal<IList<IReferer>>();

        #region Overrides of HaStrategy

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <param name="loadBalance">负载均衡器。</param>
        /// <returns>RPC响应。</returns>
        public override async Task<IResponse> Call(IRequest request, ILoadBalance loadBalance)
        {
            var referers = await SelectReferers(request, loadBalance);
            var referUrl = referers.First().Url;
            string tryCountString;
            var tryCount = 0;
            if (referUrl.Parameters.TryGetValue("tryCount", out tryCountString))
            {
                int.TryParse(tryCountString, out tryCount);
            }
            if (tryCount < 0)
                tryCount = 0;

            for (var i = 0; i < tryCount; i++)
            {
                var referer = referers[i % referers.Count];
                try
                {
                    return await referer.Call(request);
                }
                catch
                {
                    if (i >= tryCount)
                        throw;
                }
            }
            throw new RpcException("FailoverHaStrategy.call should not come here!");
        }

        #endregion Overrides of HaStrategy

        protected async Task<IList<IReferer>> SelectReferers(IRequest request, ILoadBalance loadBalance)
        {
            var referers = _referersHolder.Value;
            referers.Clear();
            await loadBalance.SelectToHolder(request, referers);
            return referers;
        }
    }
}