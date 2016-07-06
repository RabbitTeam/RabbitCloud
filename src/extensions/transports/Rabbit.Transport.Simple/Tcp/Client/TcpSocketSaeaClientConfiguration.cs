using Rabbit.Transport.Simple.Tcp.Framing;
using System;
using System.Net.Sockets;

namespace Rabbit.Transport.Simple.Tcp.Client
{
    public sealed class TcpSocketSaeaClientConfiguration
    {
        public TcpSocketSaeaClientConfiguration()
        {
            InitialPooledBufferCount = 4;
            ReceiveBufferSize = 8192;
            SendBufferSize = 8192;
            ReceiveTimeout = TimeSpan.Zero;
            SendTimeout = TimeSpan.Zero;
            NoDelay = true;
            LingerState = new LingerOption(false, 0); // The socket will linger for x seconds after Socket.Close is called.

            FrameBuilder = new LengthPrefixedFrameBuilder();
        }

        public int InitialPooledBufferCount { get; set; }
        public int ReceiveBufferSize { get; set; }
        public int SendBufferSize { get; set; }
        public TimeSpan ReceiveTimeout { get; set; }
        public TimeSpan SendTimeout { get; set; }
        public bool NoDelay { get; set; }
        public LingerOption LingerState { get; set; }

        public IFrameBuilder FrameBuilder { get; set; }
    }
}
