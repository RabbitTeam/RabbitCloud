using Microsoft.Extensions.Logging;
using NetMQ.Sockets;
using RabbitCloud.Rpc.Abstractions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace RabbitCloud.Rpc.NetMQ.Internal
{
    public interface IResponseSocketFactory : IDisposable
    {
        ResponseSocket GetResponseSocket(string protocol, IPEndPoint ipEndPoint);
    }

    public class ResponseSocketFactory : IResponseSocketFactory
    {
        private readonly ILogger<ResponseSocketFactory> _logger;

        #region Field

        private readonly ConcurrentDictionary<string, Lazy<ResponseSocket>> _responseSockets = new ConcurrentDictionary<string, Lazy<ResponseSocket>>(StringComparer.OrdinalIgnoreCase);

        #endregion Field

        #region Constructor

        public ResponseSocketFactory(ILogger<ResponseSocketFactory> logger = null)
        {
            _logger = logger ?? NullLogger<ResponseSocketFactory>.Instance;
        }

        #endregion Constructor

        #region Implementation of IResponseSocketFactory

        public ResponseSocket GetResponseSocket(string protocol, IPEndPoint ipEndPoint)
        {
            var address = $"{protocol}://{ipEndPoint.Address}:{ipEndPoint.Port}";

            return _responseSockets
                .GetOrAdd(address, k => new Lazy<ResponseSocket>(() =>
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"create ResponseSocket bind to '{address}'");

                    var responseSocket = new ResponseSocket();
                    responseSocket.Bind(address);
                    return responseSocket;
                }))
                .Value;
        }

        #endregion Implementation of IResponseSocketFactory

        #region IDisposable

        public void Dispose()
        {
            foreach (var value in _responseSockets.Values)
            {
                try
                {
                    value.Value.Dispose();
                }
                catch (Exception exception)
                {
                    _logger.LogError(0, exception, $"Dispose '{value.Value.Options.LastEndpoint}' throw exception.");
                }
            }
            _responseSockets.Clear();
        }

        #endregion IDisposable
    }

    public static class ResponseSocketFactoryExtensions
    {
        public static ResponseSocket GetResponseSocket(this IResponseSocketFactory factory, IPEndPoint ipEndPoint)
        {
            return factory.GetResponseSocket("tcp", ipEndPoint);
        }

        public static ResponseSocket GetResponseSocket(this IResponseSocketFactory factory, string ip, int port)
        {
            return factory.GetResponseSocket("tcp", new IPEndPoint(IPAddress.Parse(ip), port));
        }
    }
}