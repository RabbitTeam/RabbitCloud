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
        /// 发送传输消息。
        /// </summary>
        /// <param name="message">传输消息。</param>
        /// <returns>一个任务。</returns>
        Task SendAsync(TransportMessage message);

        /// <summary>
        /// 根据消息Id接收一个传输消息。
        /// </summary>
        /// <param name="messageId">消息Id。</param>
        /// <returns>传输消息。</returns>
        Task<TransportMessage> ReceiveAsync(string messageId);
    }
}