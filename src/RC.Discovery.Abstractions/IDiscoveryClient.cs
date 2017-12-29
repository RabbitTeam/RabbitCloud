using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Discovery.Abstractions
{
    public interface IDiscoveryClient : IDisposable
    {
        string Description { get; }

        /// <summary>
        /// all serviceId
        /// </summary>
        IReadOnlyList<string> Services { get; }

        IReadOnlyList<IServiceInstance> GetInstances(string serviceName);
    }
}