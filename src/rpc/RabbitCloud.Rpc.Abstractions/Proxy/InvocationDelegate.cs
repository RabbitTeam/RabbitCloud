using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    /// <summary>
    /// 一个抽象的调用委托。
    /// </summary>
    /// <param name="proxy">实例。</param>
    /// <param name="method">方法信息。</param>
    /// <param name="args">调用参数。</param>
    /// <returns>调用结果。</returns>
    public delegate Task<object> InvocationDelegate(object proxy, MethodInfo method, object[] args);
}