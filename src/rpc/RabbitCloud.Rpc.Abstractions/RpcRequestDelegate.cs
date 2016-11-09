using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 可以处理Rpc请求的函数。
    /// </summary>
    /// <param name="context">Rpc上下文。</param>
    /// <returns>处理请求的任务。</returns>
    public delegate Task RpcRequestDelegate(RpcContext context);
}