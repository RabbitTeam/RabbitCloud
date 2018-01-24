using System.Threading.Tasks;

namespace Rabbit.Go
{
    public interface IRetryer
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
    }
}