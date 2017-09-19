using Rabbit.Cloud.Discovery.Abstractions;
using System.Threading.Tasks;

namespace RC.Cluster.Abstractions.LoadBalance
{
    public interface IServiceInstanceChoose
    {
        Task<IServiceInstance> ChooseAsync(string serviceName);
    }
}