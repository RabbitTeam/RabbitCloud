using System.Threading.Tasks;

namespace Rabbit.Transport.Simple.Tcp.Server
{
    public interface ITcpSocketSaeaServerMessageDispatcher
    {
        Task OnSessionStarted(TcpSocketSaeaSession session);
        Task OnSessionDataReceived(TcpSocketSaeaSession session, byte[] data, int offset, int count);
        Task OnSessionClosed(TcpSocketSaeaSession session);
    }
}
