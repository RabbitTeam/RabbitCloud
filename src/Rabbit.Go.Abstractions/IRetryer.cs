using System;
using System.Threading.Tasks;

namespace Rabbit.Go
{
    public interface IRetryer : ICloneable
    {
        Task<bool> IsContinueAsync(RetryableException retryableException);
    }

    public class EmptyRetryer : IRetryer
    {
        #region Implementation of IRetryer

        public Task<bool> IsContinueAsync(RetryableException retryableException)
        {
            return Task.FromResult(false);
        }

        #endregion Implementation of IRetryer

        #region Implementation of ICloneable

        /// <inheritdoc />
        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return this;
        }

        #endregion Implementation of ICloneable
    }

    public static class RetryerExtensions
    {
        public static IRetryer CloneRetryer(this IRetryer retryer)
        {
            return (IRetryer)retryer.Clone();
        }
    }
}