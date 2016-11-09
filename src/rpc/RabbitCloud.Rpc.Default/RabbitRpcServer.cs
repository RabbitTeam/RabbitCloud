using Cowboy.Sockets.Tcp.Server;
using Newtonsoft.Json.Linq;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Features;
using RabbitCloud.Rpc.Abstractions.Hosting.Server;
using System.Linq;
using System.Net;
using System.Text;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitRpcServer : IRpcServer
    {
        private TcpSocketSaeaServer _tcpServer;
        private readonly ICodec _codec;

        public RabbitRpcServer()
        {
            Features = new RpcFeatureCollection();
            _codec = new JsonCodec();
        }

        #region Implementation of IRpcServer

        /// <summary>
        /// 特性集合。
        /// </summary>
        public IRpcFeatureCollection Features { get; }

        /// <summary>
        /// 启动服务器。
        /// </summary>
        /// <typeparam name="TContext">上下文类型。</typeparam>
        /// <param name="application">应用程序实例。</param>
        public void Start<TContext>(IRpcApplication<TContext> application)
        {
            _tcpServer = new TcpSocketSaeaServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9981), async (session, data, offset, count) =>
            {
                Features.Set<IRpcConnectionFeature>(new RpcConnectionFeature
                {
                    LocalIpAddress = session.LocalEndPoint.Address,
                    LocalPort = session.LocalEndPoint.Port,
                    RemoteIpAddress = session.RemoteEndPoint.Address,
                    RemotePort = session.RemoteEndPoint.Port
                });

                {
                    var buffer = data.Skip(offset).Take(count).ToArray();
                    var jObj = JObject.Parse(Encoding.UTF8.GetString(buffer));
                    Features.Set<IRpcRequestFeature>(new RpcRequestFeature
                    {
                        Body = _codec.Decode(jObj, typeof(Invocation)),
                        Path = jObj.Value<string>("Path"),
                        PathBase = "/",
                        QueryString = jObj.Value<string>("QueryString"),
                        Scheme = jObj.Value<string>("Scheme")
                    });
                }
                Features.Set<IRpcResponseFeature>(new RpcResponseFeature());
                var context = application.CreateContext(Features);
                try
                {
                    await application.ProcessRequestAsync(context);
                }
                finally
                {
                    application.DisposeContext(context, null);
                }
            });
            _tcpServer.Listen();
        }

        #endregion Implementation of IRpcServer

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            _tcpServer?.Dispose();
        }

        #endregion Implementation of IDisposable
    }
}