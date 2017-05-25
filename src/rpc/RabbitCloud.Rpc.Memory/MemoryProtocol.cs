using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Concurrent;

namespace RabbitCloud.Rpc.Memory
{
    public class MemoryProtocol : IProtocol
    {
        private readonly ConcurrentDictionary<string, Lazy<IExporter>> _exporters = new ConcurrentDictionary<string, Lazy<IExporter>>();

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _exporters.Clear();
        }

        #endregion Implementation of IDisposable

        #region Implementation of IProtocol

        public IExporter Export(ExportContext context)
        {
            var protocolKey = GetProtocolKey(context);
            return _exporters.GetOrAdd(protocolKey, new Lazy<IExporter>(() =>
                {
                    var exporter = new MemoryExporter(context.Caller, () => _exporters.TryRemove(protocolKey, out Lazy<IExporter> _));
                    return exporter;
                }))
                .Value;
        }

        public ICaller Refer(ReferContext context)
        {
            var protocolKey = GetProtocolKey(context.ServiceKey);
            return !_exporters.TryGetValue(protocolKey, out Lazy<IExporter> exporterLazy) ? null : exporterLazy.Value.Export();
        }

        #endregion Implementation of IProtocol

        #region Private Method

        private static string GetProtocolKey(ProtocolContext context)
        {
            return GetProtocolKey(context.ServiceKey);
        }

        private static string GetProtocolKey(ServiceKey serviceKey)
        {
            return $"memory://127.0.0.1/{serviceKey}";
        }

        #endregion Private Method
    }
}