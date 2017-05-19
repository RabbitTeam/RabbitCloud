using RabbitCloud.Abstractions.Exceptions;
using RabbitCloud.Abstractions.Exceptions.Extensions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster
{
    public class DefaultCluster : ICluster
    {
        private long _available = 1;

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
                {
                    await Task.Yield();
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