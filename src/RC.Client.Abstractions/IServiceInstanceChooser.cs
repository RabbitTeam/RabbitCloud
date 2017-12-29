using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IServiceInstanceChooser
    {
        IServiceInstance Choose(IReadOnlyList<IServiceInstance> instances);
    }
}