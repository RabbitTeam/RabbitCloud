using Consul;
using Microsoft.Extensions.Logging;
using RabbitCloud.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Consul
{
    public class HeartbeatManager : IDisposable
    {
        private readonly IConsulClient _consulClient;
        private readonly ILogger<HeartbeatManager> _logger;
        private readonly ISet<string> _checkIds = new HashSet<string>();
        private readonly Timer _timer;

        public HeartbeatManager(IConsulClient consulClient, ILogger<HeartbeatManager> logger = null)
        {
            _consulClient = consulClient;
            logger = logger ?? NullLogger<HeartbeatManager>.Instance;
            _logger = logger;

            //比consul设置的ttl减少3秒
            var timeSpan = ConsulConstants.TtlInterval.Subtract(TimeSpan.FromSeconds(3));
            _timer = new Timer(async s =>
            {
                string[] ids;
                lock (_checkIds)
                    ids = _checkIds.ToArray();

                var nodeName = await consulClient.Agent.GetNodeName();
                Parallel.ForEach(ids, async id =>
                {
                    await PassTtl(id, nodeName);
                });
            }, null, TimeSpan.Zero, timeSpan);
        }

        public async Task AddHeartbeat(string serviceId, bool immediatelyPass = true)
        {
            var checkId = "service:" + serviceId;
            lock (_checkIds)
            {
                _checkIds.Add(checkId);
            }

            if (immediatelyPass)
                await PassTtl(checkId);
        }

        public void RemoveHeartbeat(string serviceId)
        {
            lock (_checkIds)
            {
                _checkIds.Add("service:" + serviceId);
            }
        }

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }

        #endregion IDisposable

        #region Private Method

        private async Task PassTtl(string id, string nodeName = null)
        {
            if (nodeName == null)
                nodeName = await _consulClient.Agent.GetNodeName();

            for (var i = 0; i < 3; i++)
            {
                try
                {
                    await _consulClient.Agent.PassTTL(id, nodeName);
                }
                catch (Exception exception)
                {
                    _logger.LogError(0, exception, $"pass TTL failure.id:{id},try count:{i + 1}");
                }
            }
        }

        #endregion Private Method
    }
}