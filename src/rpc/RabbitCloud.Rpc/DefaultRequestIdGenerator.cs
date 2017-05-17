using RabbitCloud.Abstractions.Utilities;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.Threading;

namespace RabbitCloud.Rpc
{
    public class DefaultRequestIdGenerator : IRequestIdGenerator
    {
        private static long _offset;
        protected static readonly int Bits = 20;
        protected static readonly long MaxCountPerMillis = 1 << Bits;

        #region Implementation of IRequestIdGenerator

        public long GetRequestId()
        {
            var currentTime = DateTime.UtcNow.GetMillisecondsTimeStamp();

            var count = Interlocked.Increment(ref _offset);
            while (count >= MaxCountPerMillis)
            {
                lock (this)
                {
                    if (_offset >= MaxCountPerMillis)
                    {
                        Interlocked.Exchange(ref _offset, 0);
                    }
                }
                count = Interlocked.Increment(ref _offset);
            }

            return (currentTime << Bits) + count;
        }

        #endregion Implementation of IRequestIdGenerator
    }
}