using RabbitCloud.Config.Abstractions;
using RabbitCloud.Rpc.Abstractions.Proxy;

namespace RabbitCloud.Config
{
    public class ApplicationModel : IApplicationModel
    {
        private readonly IProxyFactory _proxyFactory;

        public ApplicationModel(IProxyFactory proxyFactory)
        {
            _proxyFactory = proxyFactory;
        }

        public ProtocolEntry[] Protocols { get; set; }
        public RegistryTableEntry[] RegistryTables { get; set; }
        public CallerEntry[] CallerEntries { get; set; }
        public ServiceEntry[] ServiceEntries { get; set; }

        public T Referer<T>(string id)
        {
            return _proxyFactory.GetProxy<T>(this.GetCallerEntry(id).Caller);
        }

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (CallerEntries != null)
                foreach (var disposable in CallerEntries)
                {
                    disposable.Dispose();
                }
            if (ServiceEntries != null)
                foreach (var disposable in ServiceEntries)
                {
                    disposable.Dispose();
                }
            if (RegistryTables != null)
                foreach (var disposable in RegistryTables)
                {
                    disposable.Dispose();
                }
            if (Protocols != null)
                foreach (var disposable in Protocols)
                {
                    disposable.Dispose();
                }
        }

        #endregion IDisposable
    }
}