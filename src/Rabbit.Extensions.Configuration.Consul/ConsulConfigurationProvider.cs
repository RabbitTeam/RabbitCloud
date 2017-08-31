using Consul;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Extensions.Configuration.Consul
{
    internal class ConsulConfigurationProvider : ConfigurationProvider
    {
        private readonly ConsulConfigurationSource _source;

        private ulong _lastIndex;
        private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

        public ConsulConfigurationProvider(ConsulConfigurationSource source)
        {
            _source = source;
        }

        #region Overrides of ConfigurationProvider

        /// <inheritdoc />
        /// <summary>
        /// Loads (or reloads) the data for this provider.
        /// </summary>
        public override void Load()
        {
            Task.Run(async () =>
            {
                _lastIndex = await Load(null);
                TryWatch(OnReload);
            }).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        /// <summary>
        /// Attempts to find a value with the given key, returns true if one is found, false otherwise.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <param name="value">The value found at key if one is found.</param>
        /// <returns>True if key has a value, false otherwise.</returns>
        public override bool TryGet(string key, out string value)
        {
            try
            {
                _readerWriterLockSlim.EnterReadLock();
                return base.TryGet(key, out value);
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        #endregion Overrides of ConfigurationProvider

        #region Private Method

        private void TryWatch(Action callback)
        {
            if (!_source.ReloadOnChange)
                return;

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var result = await Load(_lastIndex);

                    //no modify return
                    if (result == _lastIndex)
                        continue;

                    _lastIndex = result;
                    callback();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private async Task<ulong> Load(ulong? index)
        {
            var result = await _source.ConsulClient.KV.List(_source.Path,
                index == null ? null : new QueryOptions { WaitIndex = index.Value });

            if (!_source.Optional && result.StatusCode == HttpStatusCode.NotFound)
                throw new ArgumentException($"node: '{_source.Path}' not found.");

            Set(result);

            return result.LastIndex;
        }

        private void Set(QueryResult<KVPair[]> result)
        {
            if (result?.Response == null)
                return;
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                Data = result.Response.ToDictionary(i => _source.KeyConver(i.Key), i => _source.ValueConver(i.Value));
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        #endregion Private Method
    }
}