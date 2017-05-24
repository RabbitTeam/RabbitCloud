using Microsoft.Extensions.Logging;
using RabbitCloud.Abstractions.Exceptions;
using RabbitCloud.Abstractions.Exceptions.Extensions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster
{
    public class DefaultCluster : ICluster
    {
        #region Field

        private readonly ILogger<DefaultCluster> _logger;
        private long _available = 1;

        #endregion Field

        #region Constructor

        public DefaultCluster(IEnumerable<ICaller> callers, ILoadBalance loadBalance, IHaStrategy haStrategy, ILogger<DefaultCluster> logger)
        {
            Callers = callers.ToArray();
            loadBalance.Callers = Callers;
            LoadBalance = loadBalance;
            HaStrategy = haStrategy;
            _logger = logger;
        }

        #endregion Constructor

        #region Implementation of ICaller

        public bool IsAvailable => Interlocked.Read(ref _available) == 1;

        public async Task<IResponse> CallAsync(IRequest request)
        {
            if (Interlocked.Read(ref _available) != 1)
                return CallFailure(request, new RabbitServiceException());
            try
            {
                return await HaStrategy.CallAsync(request, LoadBalance);
            }
            catch (Exception e)
            {
                return CallFailure(request, e);
            }
        }

        #endregion Implementation of ICaller

        #region Implementation of ICluster

        public ICaller[] Callers { get; set; }
        public ILoadBalance LoadBalance { get; set; }
        public IHaStrategy HaStrategy { get; set; }

        #endregion Implementation of ICluster

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                while (Interlocked.Exchange(ref _available, 0) != 0)
                    await Task.Yield();

                foreach (var caller in Callers)
                {
                    try
                    {
                        (caller as IDisposable)?.Dispose();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(0, e, $"dispose '{caller}' throw exception.");
                    }
                }
            })
            .Wait();
        }

        #endregion IDisposable

        #region Private Method

        private static Response CallFailure(IRequest request, Exception exception)
        {
            if (exception.IsBusinessException())
                throw exception;

            return new Response(request)
            {
                Exception = exception
            };
        }

        #endregion Private Method
    }
}