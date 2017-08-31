using System;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions
{
    public interface IRegistryService<in T> : IDisposable where T : IRegistration
    {
        Task RegisterAsync(T registration);

        Task DeregisterAsync(T registration);
    }
}