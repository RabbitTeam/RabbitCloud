using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Rpc.Abstractions.Cluster;
using RabbitCloud.Rpc.Cluster;
using RabbitCloud.Rpc.Cluster.HA;
using RabbitCloud.Rpc.Cluster.LoadBalance;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Config
{
    public class DefaultClusterFactory : IClusterFactory
    {
        private readonly IEnumerable<IClusterProvider> _clusterProviders;
        private readonly IEnumerable<ILoadBalanceProvider> _loadBalanceProviders;
        private readonly IEnumerable<IHaStrategyProvider> _haStrategyProviders;

        public DefaultClusterFactory(IEnumerable<IClusterProvider> clusterProviders, IEnumerable<ILoadBalanceProvider> loadBalanceProviders, IEnumerable<IHaStrategyProvider> haStrategyProviders)
        {
            _clusterProviders = clusterProviders;
            _loadBalanceProviders = loadBalanceProviders;
            _haStrategyProviders = haStrategyProviders;
        }

        #region Implementation of IClusterFactory

        public ICluster CreateCluster(string clusterName, string loadBalanceName, string haStrategyName)
        {
            var clusterProvider =
                _clusterProviders.SingleOrDefault(
                    i => string.Equals(i.Name, clusterName, StringComparison.OrdinalIgnoreCase));
            var loadBalanceProvider =
                _loadBalanceProviders.SingleOrDefault(
                    i => string.Equals(i.Name, loadBalanceName, StringComparison.OrdinalIgnoreCase));
            var haStrategyProvider =
                _haStrategyProviders.SingleOrDefault(
                    i => string.Equals(i.Name, haStrategyName, StringComparison.OrdinalIgnoreCase));

            var cluster = clusterProvider.CreateCluster();
            cluster.LoadBalance = loadBalanceProvider.CreateLoadBalance();
            cluster.HaStrategy = haStrategyProvider.CreateHaStrategy();

            return cluster;
        }

        #endregion Implementation of IClusterFactory
    }

    public class DefaultClusterProvider : IClusterProvider
    {
        #region Implementation of IClusterProvider

        public string Name { get; } = "Default";

        public ICluster CreateCluster()
        {
            return new DefaultCluster();
        }

        #endregion Implementation of IClusterProvider
    }

    public class FailfastHaStrategyProvider : IHaStrategyProvider
    {
        #region Implementation of IHaStrategyProvider

        public string Name { get; } = "Failfast";

        public IHaStrategy CreateHaStrategy()
        {
            return new FailfastHaStrategy();
        }

        #endregion Implementation of IHaStrategyProvider
    }

    public class FailoverHaStrategyProvider : IHaStrategyProvider
    {
        #region Implementation of IHaStrategyProvider

        public string Name { get; } = "Failover";

        public IHaStrategy CreateHaStrategy()
        {
            return new FailoverHaStrategy();
        }

        #endregion Implementation of IHaStrategyProvider
    }

    public class RoundRobinLoadBalanceProvider : ILoadBalanceProvider
    {
        #region Implementation of ILoadBalanceProvider

        public string Name { get; } = "RoundRobin";

        public ILoadBalance CreateLoadBalance()
        {
            return new RoundRobinLoadBalance();
        }

        #endregion Implementation of ILoadBalanceProvider
    }

    public class RandomLoadBalanceProvider : ILoadBalanceProvider
    {
        #region Implementation of ILoadBalanceProvider

        public string Name { get; } = "Random";

        public ILoadBalance CreateLoadBalance()
        {
            return new RandomLoadBalance();
        }

        #endregion Implementation of ILoadBalanceProvider
    }
}