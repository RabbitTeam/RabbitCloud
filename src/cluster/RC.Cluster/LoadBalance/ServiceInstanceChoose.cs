using Rabbit.Cloud.Discovery.Abstractions;
using RC.Cluster.Abstractions.LoadBalance;
using System.Threading.Tasks;

namespace RC.Cluster.LoadBalance
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