using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的调用者。
    /// </summary>
    public interface IInvoker : INode
    {
        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="invocation">调用信息。</param>
        /// <returns>返回结果。</returns>
        Task<IResult> Invoke(IInvocation invocation);
    }
}