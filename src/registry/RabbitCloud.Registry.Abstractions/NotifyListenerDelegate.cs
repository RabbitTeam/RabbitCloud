using RabbitCloud.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions
{
    /// <summary>
    /// 通知监听委托。
    /// </summary>
    /// <param name="registryUrl">注册时的Url。</param>
    /// <param name="urls">发生变更的Url集合。</param>
    /// <returns>一个任务。</returns>
    public delegate Task NotifyListenerDelegate(Url registryUrl, Url[] urls);
}