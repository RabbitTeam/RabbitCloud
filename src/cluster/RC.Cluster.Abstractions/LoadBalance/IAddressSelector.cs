using System;
using System.Threading.Tasks;

namespace RC.Cluster.Abstractions.LoadBalance
{
    public interface IAddressSelector
    {
        Task<Uri> SelectAsync(string serviceName);
    }
}