using Microsoft.Extensions.Logging;
using RabbitCloud.Abstractions.Exceptions;
using RabbitCloud.Abstractions.Exceptions.Extensions;
using RabbitCloud.Abstractions.Logging;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.HA
{
    public class FailoverHaStrategy : IHaStrategy
    {
        private readonly ILogger<FailoverHaStrategy> _logger;

        public FailoverHaStrategy(ILogger<FailoverHaStrategy> logger = null)
        {
            _logger = logger ?? NullLogger<FailoverHaStrategy>.Instance;
        }

        #region Implementation of IHaStrategy

        public async Task<IResponse> CallAsync(IRequest request, ILoadBalance loadBalance)
        {
            var count = loadBalance.Callers.Count();

            if (count == 0)
                throw new RabbitServiceException($"FailoverHaStrategy No referers for request:{request}, loadbalance:{loadBalance}");

            Exception exception = null;

            //todo:添加重试机制
            for (var i = 0; i < count; i++)
            {
                var caller = loadBalance.Select(request);
                try
                {
                    return await caller.CallAsync(request);
                }
                catch (Exception e)
                {
                    if (e.IsBusinessException())
                        throw;

                    _logger.LogError(0, e, $"FailoverHaStrategy Call false for request:{request} error={e.Message}");
                }
            }

            if (exception != null)
                throw exception;

            throw new RabbitFrameworkException("FailoverHaStrategy.call should not come here!");
        }

        #endregion Implementation of IHaStrategy
    }
}