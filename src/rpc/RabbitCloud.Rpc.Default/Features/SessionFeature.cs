using Cowboy.Sockets.Tcp.Server;

namespace RabbitCloud.Rpc.Default.Features
{
    public interface ISessionFeature
    {
        TcpSocketSaeaSession Session { get; set; }
    }

    public class SessionFeature : ISessionFeature
    {
        #region Implementation of ISessionFeature

        public TcpSocketSaeaSession Session { get; set; }

        #endregion Implementation of ISessionFeature
    }
}