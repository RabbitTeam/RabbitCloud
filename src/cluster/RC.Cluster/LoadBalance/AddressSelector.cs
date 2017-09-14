using RC.Cluster.Abstractions.LoadBalance;
using System;
using System.Threading.Tasks;

namespace RC.Cluster.LoadBalance
{
    public class AddressSelector : IAddressSelector
    {
        #region Implementation of IAddressSelector

        public virtual Task<Uri> SelectAsync(string serviceName)
        {
            return Task.FromResult(Select(serviceName));
        }

        #endregion Implementation of IAddressSelector

        protected virtual Uri Select(string serviceName)
        {
            return null;
        }
    }
}