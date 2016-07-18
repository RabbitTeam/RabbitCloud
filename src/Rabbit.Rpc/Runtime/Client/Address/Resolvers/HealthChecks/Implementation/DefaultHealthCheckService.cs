using Rabbit.Rpc.Address;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Client.Address.Resolvers.HealthChecks.Implementation
{
    public class DefaultHealthCheckService : IHealthCheckService, IDisposable
    {
        private readonly ConcurrentDictionary<string, MonitorEntry> _dictionary = new ConcurrentDictionary<string, MonitorEntry>();
        private readonly Timer _timer;

        public DefaultHealthCheckService()
        {
            var timeSpan = TimeSpan.FromSeconds(10);
            _timer = new Timer(s =>
           {
               foreach (var item in _dictionary.ToArray().Select(i => i.Value))
               {
                   var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                   var socketAsyncEventArgs = new SocketAsyncEventArgs
                   {
                       RemoteEndPoint = item.EndPoint,
                       UserToken = new KeyValuePair<MonitorEntry, Socket>(item, socket)
                   };
                   socketAsyncEventArgs.Completed += (sender, e) =>
                   {
                       var isHealth = e.SocketError == SocketError.Success;

                       var token = (KeyValuePair<MonitorEntry, Socket>)e.UserToken;
                       token.Key.Health = isHealth;

                       e.Dispose();
                       token.Value.Dispose();
                   };

                   socket.ConnectAsync(socketAsyncEventArgs);
               }
           }, null, timeSpan, timeSpan);
        }

        #region Implementation of IHealthCheckService

        /// <summary>
        /// 监控一个地址。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        public Task Monitor(AddressModel address)
        {
            return Task.Run(() => { _dictionary.GetOrAdd(address.ToString(), (k) => new MonitorEntry(address)); });
        }

        /// <summary>
        /// 判断一个地址是否健康。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>健康返回true，否则返回false。</returns>
        public Task<bool> IsHealth(AddressModel address)
        {
            return Task.Run(() =>
            {
                var key = address.ToString();
                MonitorEntry entry;

                return !_dictionary.TryGetValue(key, out entry) || entry.Health;
            });
        }

        #endregion Implementation of IHealthCheckService

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _timer.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Help Class

        protected class MonitorEntry
        {
            public MonitorEntry(AddressModel addressModel)
            {
                EndPoint = addressModel.CreateEndPoint();
                Health = true;
            }

            public EndPoint EndPoint { get; set; }
            public bool Health { get; set; }
        }

        #endregion Help Class
    }
}