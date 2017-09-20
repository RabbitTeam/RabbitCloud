using Rabbit.Cloud.Cluster.Abstractions.LoadBalance;
using Rabbit.Cloud.Discovery.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Cluster.LoadBalance
{
    public class ServiceInstanceChoose : IServiceInstanceChoose
    {
        #region Implementation of IServiceInstanceChoose

        public virtual Task<IServiceInstance> ChooseAsync(string serviceName)
        {
            return Task.FromResult(Choose(serviceName));
        }

        #endregion Implementation of IServiceInstanceChoose

        protected virtual IServiceInstance Choose(string serviceName)
        {
            return null;
        }
    }
}