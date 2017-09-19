using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Extensions.Consul
{
    public class HeartbeatManager : IDisposable
    {
        private readonly IConsulClient _consulClient;
        private readonly ILogger<HeartbeatManager> _logger;
        private readonly ConcurrentDictionary<string, CheckEntry> _checkEntries = new ConcurrentDictionary<string, CheckEntry>(StringComparer.OrdinalIgnoreCase);
        private readonly Timer _timer;

        public HeartbeatManager(IConsulClient consulClient, ILogger<HeartbeatManager> logger = null)
        {
            _consulClient = consulClient;
            logger = logger ?? NullLogger<HeartbeatManager>.Instance;
            _logger = logger;

            _timer = new Timer(async s =>
            {
                var checkEntries = _checkEntries.Values.Where(i => i.NeedTtl()).ToArray();

                var nodeName = await consulClient.Agent.GetNodeName();
                Parallel.ForEach(checkEntries, async entry =>
                {
                    await PassTtl(entry.CheckId, nodeName);
                });
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public async Task AddHeartbeat(string serviceId, TimeSpan interval, bool immediatelyPass = true)
        {
            var checkId = "service:" + serviceId;

            _checkEntries.TryAdd(serviceId, new CheckEntry(serviceId, interval));

            if (immediatelyPass)
                await PassTtl(checkId);
        }

        public void RemoveHeartbeat(string serviceId)
        {
            _checkEntries.TryRemove(serviceId, out var _);
        }

        #region IDisposable

        /// <inheritdoc />
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

        #region Help Type

        private class CheckEntry
        {
            public CheckEntry(string serviceId, TimeSpan interval)
            {
                Interval = interval;
                LatestTtlTime = DateTime.Now;
                CheckId = "service:" + serviceId;
            }

            public string CheckId { get; }
            private DateTime LatestTtlTime { get; }
            private TimeSpan Interval { get; }

            public bool NeedTtl()
            {
                return DateTime.Now > LatestTtlTime.Add(Interval);
            }
        }

        #endregion Help Type
    }
}