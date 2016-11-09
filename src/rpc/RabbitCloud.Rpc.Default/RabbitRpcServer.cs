using Cowboy.Sockets.Tcp.Server;
using Newtonsoft.Json.Linq;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Features;
using RabbitCloud.Rpc.Abstractions.Hosting.Server;
using RabbitCloud.Rpc.Abstractions.Hosting.Server.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitRpcServer : IRpcServer
    {
        private readonly IList<TcpSocketSaeaServer> _tcpServers = new List<TcpSocketSaeaServer>();
        private readonly ICodec _codec;
        private readonly IServerAddressesFeature _serverAddressesFeature;

        public RabbitRpcServer()
        {
            Features = new RpcFeatureCollection();
            _serverAddressesFeature = new ServerAddressesFeature();
            Features.Set(_serverAddressesFeature);
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
            Task.Run(async () =>
            {
                foreach (var address in _serverAddressesFeature.Addresses)
                {
                    var ipEndPoints = await GetIpEndPoints(address);
                    if (ipEndPoints == null)
                        continue;
                    foreach (var ipEndPoint in ipEndPoints)
                    {
                        var tcpServer = StartTcpServer(ipEndPoint, async (session, data, offset, count) =>
                        {
                            SetFeatures(session, data, offset, count);
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
                        _tcpServers.Add(tcpServer);
                    }
                }
            }).Wait();
        }

        #endregion Implementation of IRpcServer

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            foreach (var server in _tcpServers)
            {
                server.Dispose();
            }
            _tcpServers.Clear();
        }

        #endregion Implementation of IDisposable

        #region Private Method

        private static TcpSocketSaeaServer StartTcpServer(IPEndPoint ipEndPoint, Func<TcpSocketSaeaSession, IEnumerable<byte>, int, int, Task> onSessionDataReceived)
        {
            var tcpServer = new TcpSocketSaeaServer(ipEndPoint, onSessionDataReceived);
            tcpServer.Listen();
            return tcpServer;
        }

        private static async Task<IEnumerable<IPEndPoint>> GetIpEndPoints(string address)
        {
            var temp = address.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            var host = temp[0];
            var port = 0;
            if (temp.Length > 1)
                int.TryParse(temp[1], out port);
            var ipAddresses = await Dns.GetHostAddressesAsync(host);
            return ipAddresses.Select(i => new IPEndPoint(i, port));
        }

        private void SetFeatures(TcpSocketSaeaSession session, IEnumerable<byte> data, int offset, int count)
        {
            //RpcConnectionFeature
            {
                Features.Set<IRpcConnectionFeature>(new RpcConnectionFeature
                {
                    LocalIpAddress = session.LocalEndPoint.Address,
                    LocalPort = session.LocalEndPoint.Port,
                    RemoteIpAddress = session.RemoteEndPoint.Address,
                    RemotePort = session.RemoteEndPoint.Port
                });
            }

            //RpcRequestFeature
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

            //RpcResponseFeature
            {
                Features.Set<IRpcResponseFeature>(new RpcResponseFeature());
            }
        }

        #endregion Private Method
    }
}