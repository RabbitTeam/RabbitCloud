using System;
using System.Collections.Generic;

namespace Rabbit.Discovery.Abstractions
{
    public interface IDiscoveryClient : IDisposable
    {
        string Description { get; }
        /// <summary>
        /// all serviceId
        /// </summary>
        IReadOnlyCollection<string> Services { get; }

        IReadOnlyCollection<IServiceInstance> GetInstances(string serviceId);
    }
}