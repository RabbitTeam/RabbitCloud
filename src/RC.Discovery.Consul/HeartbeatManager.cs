using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Discovery.Consul
{
    public class HeartbeatManager : IDisposable
    {
        private readonly IConsulClient _consulClient;
        private readonly ILogger<HeartbeatManager> _logger;
        private readonly ConcurrentDictionary<string, CheckEntry> _checkEntries = new ConcurrentDictionary<string, CheckEntry>(StringComparer.OrdinalIgnoreCase);
        private bool _isDisposable;

        public HeartbeatManager(IConsulClient consulClient, ILogger<HeartbeatManager> logger = null)
        {
            _consulClient = consulClient;
            logger = logger ?? NullLogger<HeartbeatManager>.Instance;
            _logger = logger;

            Task.Factory.StartNew(async () =>
            {
                while (!_isDisposable)
                {
                    var checkEntries = _checkEntries.Values.Where(i => i.NeedTtl()).ToArray();
                    if (checkEntries.Any())
                    {
                        var nodeName = await consulClient.Agent.GetNodeName();
                        var tasks = checkEntries.ToDictionary(i => i, entry => PassTtl(entry.CheckId, nodeName));

                        await Task.WhenAll(tasks.Values);

                        foreach (var item in tasks)
                        {
                            if (await item.Value)
                                item.Key.Ttl();
                        }
                    }
                    //todo:计算出间隔时间，而不是固定写死
                    await Task.Delay(5000);
                }
            }, TaskCreationOptions.LongRunning);
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
            _isDisposable = true;
        }

        #endregion IDisposable

        #region Private Method

        private async Task<bool> PassTtl(string id, string nodeName = null)
        {
            if (nodeName == null)
                nodeName = await _consulClient.Agent.GetNodeName();

            for (var i = 0; i < 3; i++)
            {
                try
                {
                    await _consulClient.Agent.PassTTL(id, nodeName);
                    return true;
                }
                catch (Exception exception)
                {
                    _logger.LogError(0, exception, $"pass TTL failure.id:{id},try count:{i + 1}");
                }
            }
            return false;
        }

        #endregion Private Method

        #region Help Type

        private class CheckEntry
        {
            public CheckEntry(string serviceId, TimeSpan interval)
            {
                interval = interval.Subtract(TimeSpan.FromSeconds(2));
                if (interval < TimeSpan.FromSeconds(1))
                    interval = TimeSpan.FromSeconds(1);

                Interval = interval;
                LatestTtlTime = DateTime.Now;
                CheckId = "service:" + serviceId;
            }

            public string CheckId { get; }
            private DateTime LatestTtlTime { get; set; }
            private TimeSpan Interval { get; }

            public void Ttl()
            {
                LatestTtlTime = DateTime.Now;
            }

            public bool NeedTtl()
            {
                return DateTime.Now > LatestTtlTime.Add(Interval);
            }
        }

        #endregion Help Type
    }
}