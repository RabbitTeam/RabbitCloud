using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.Internal
{
    internal sealed class FairServiceInstanceChooser : IServiceInstanceChooser
    {
        private readonly IServiceInstanceChooser _chooser;
        private IList<IServiceInstance> _alreadyServiceInstances;

        public FairServiceInstanceChooser(IServiceInstanceChooser chooser)
        {
            _chooser = chooser;
        }

        #region Implementation of IServiceInstanceChooser

        public IServiceInstance Choose(IReadOnlyList<IServiceInstance> instances)
        {
            var serviceInstance = _chooser.Choose(GetAvailableServiceInstances(instances));

            AddAlready(serviceInstance);

            return serviceInstance;
        }

        #endregion Implementation of IServiceInstanceChooser

        #region Private Method

        private void AddAlready(IServiceInstance serviceInstance)
        {
            if (_alreadyServiceInstances == null)
                _alreadyServiceInstances = new List<IServiceInstance>();

            _alreadyServiceInstances.Add(serviceInstance);
        }

        private IReadOnlyList<IServiceInstance> GetAvailableServiceInstances(IReadOnlyList<IServiceInstance> serviceInstances)
        {
            //没有任何调用过的服务实例则全部返回，否则过滤掉已经调用过的服务实例
            if (_alreadyServiceInstances == null || !_alreadyServiceInstances.Any())
                return serviceInstances;

            //所有的服务实例都已经被调用过，则清除重新开始
            if (_alreadyServiceInstances.Count == serviceInstances.Count)
                _alreadyServiceInstances.Clear();

            return serviceInstances.Except(_alreadyServiceInstances).ToArray();
        }

        #endregion Private Method
    }
}