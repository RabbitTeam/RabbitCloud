using System.Threading.Tasks;

namespace Rabbit.Transport.Simple.Tcp.Client
{
    public interface ITcpSocketSaeaClientMessageDispatcher
    {
        Task OnServerConnected(TcpSocketSaeaClient client);
        Task OnServerDataReceived(TcpSocketSaeaClient client, byte[] data, int offset, int count);
        Task OnServerDisconnected(TcpSocketSaeaClient client);
    }
}
