using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Discovery.Abstractions
{
    public interface IRegistryService<in T> : IDisposable where T : IRegistration
    {
        Task RegisterAsync(T registration);

        Task DeregisterAsync(T registration);
    }
}