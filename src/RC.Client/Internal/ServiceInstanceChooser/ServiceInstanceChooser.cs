using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.ServiceInstanceChooser
{
    public abstract class ServiceInstanceChooser : IServiceInstanceChooser
    {
        #region Implementation of IServiceInstanceChooser

        public IServiceInstance Choose(IReadOnlyList<IServiceInstance> instances)
        {
            if (instances == null || !instances.Any())
                return null;

            return instances.Count == 1 ? instances.ElementAt(0) : DoChoose(instances);
        }

        #endregion Implementation of IServiceInstanceChooser

        protected abstract IServiceInstance DoChoose(IReadOnlyCollection<IServiceInstance> instances);
    }
}