using System.Net;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport.Channels
{
    /// <summary>
    /// 接受到消息的委托。
    /// </summary>
    /// <param name="channel">传输通道。</param>
    /// <param name="message">接收到的消息。</param>
    public delegate void ReceivedDelegate(ITransportChannel channel, object message);

    /// <summary>
    /// 一个抽象的传输通道。
    /// </summary>
    public interface ITransportChannel
    {
        /// <summary>
        /// 接收到消息的事件。
        /// </summary>
        event ReceivedDelegate Received;

        /// <summary>
        /// 是否打开。
        /// </summary>
        bool Open { get; }

        /// <summary>
        /// 本地地址。
        /// </summary>
        EndPoint LocalAddress { get; }

        /// <summary>
        /// 远程地址。
        /// </summary>
        EndPoint RemoteAddress { get; }

        /*/// <summary>
        /// 绑定到
        /// </summary>
        /// <param name="localAddress"></param>
        /// <returns></returns>
                Task BindAsync(EndPoint localAddress);*/

        /// <summary>
        /// 连接到远程服务器。
        /// </summary>
        /// <param name="remoteAddress">远程服务器地址。</param>
        /// <returns>一个任务。</returns>
        Task ConnectAsync(EndPoint remoteAddress);

        /// <summary>
        /// 连接到远程服务器。
        /// </summary>
        /// <param name="remoteAddress">远程服务器地址。</param>
        /// <param name="localAddress">本地绑定地址。</param>
        /// <returns>一个任务。</returns>
        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

        /*/// <summary>
        /// 关闭通道。
        /// </summary>
        /// <returns>一个人任务。</returns>
        Task CloseAsync();

        /// <summary>
        /// 断开频道。
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();*/

        /// <summary>
        /// 写入。
        /// </summary>
        /// <param name="message">消息对象。</param>
        /// <returns>一个人任务。</returns>
        Task WriteAsync(object message);

        /// <summary>
        /// 写入并刷新缓冲区。
        /// </summary>
        /// <param name="message">消息对象。</param>
        /// <returns>一个人任务。</returns>
        Task WriteAndFlushAsync(object message);

        /// <summary>
        /// 刷新缓冲区。
        /// </summary>
        /// <returns>传输通道。</returns>
        ITransportChannel Flush();
    }
}