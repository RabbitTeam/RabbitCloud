using Rabbit.Rpc.Messages;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport
{
    /// <summary>
    /// 一个抽象的传输客户端。
    /// </summary>
    public interface ITransportClient
    {
        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <returns>一个任务。</returns>
        Task SendAsync(RemoteInvokeMessage message);

        /// <summary>
        /// 接受指定消息id的响应消息。
        /// </summary>
        /// <param name="id">消息Id。</param>
        /// <returns>远程调用结果消息模型。</returns>
        Task<RemoteInvokeResultMessage> ReceiveAsync(string id);
    }
}