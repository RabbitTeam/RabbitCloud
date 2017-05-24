using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.Linq;

namespace RabbitCloud.Config.Abstractions
{
    public class CallerEntry : IDisposable
    {
        public RefererConfig RefererConfig { get; set; }
        public IProtocol Protocol { get; set; }
        public IRegistryTable RegistryTable { get; set; }
        public ICaller Caller { get; set; }

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            (Caller as IDisposable)?.Dispose();
        }

        #endregion IDisposable
    }

    public class ServiceEntry : IDisposable
    {
        public ServiceConfig ServiceConfig { get; set; }
        public IProtocol Protocol { get; set; }
        public IRegistryTable RegistryTable { get; set; }
        public IExporter Exporter { get; set; }

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            (Exporter as IDisposable)?.Dispose();
        }

        #endregion IDisposable
    }

    public class ProtocolEntry : IDisposable
    {
        public ProtocolConfig ProtocolConfig { get; set; }
        public IProtocol Protocol { get; set; }

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Protocol?.Dispose();
        }

        #endregion IDisposable
    }

    public class RegistryTableEntry : IDisposable
    {
        public RegistryConfig RegistryConfig { get; set; }
        public IRegistryTable RegistryTable { get; set; }

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            (RegistryTable as IDisposable)?.Dispose();
        }

        #endregion IDisposable
    }

    public interface IApplicationModel : IDisposable
    {
        ProtocolEntry[] Protocols { get; }
        RegistryTableEntry[] RegistryTables { get; }
        CallerEntry[] CallerEntries { get; }
        ServiceEntry[] ServiceEntries { get; }

        T Referer<T>(string id);
    }

    public static class ApplicationModelExtensions
    {
        public static CallerEntry GetCallerEntry(this IApplicationModel model, string id)
        {
            return model.CallerEntries.SingleOrDefault(i => string.Equals(i.RefererConfig.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public static ServiceEntry GetServiceEntry(this IApplicationModel model, string id)
        {
            return model.ServiceEntries.SingleOrDefault(i => string.Equals(i.ServiceConfig.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public static ProtocolEntry GetProtocol(this IApplicationModel model, string id)
        {
            return model.Protocols.SingleOrDefault(i => string.Equals(i.ProtocolConfig.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public static RegistryTableEntry GetRegistryTable(this IApplicationModel model, string name)
        {
            return model.RegistryTables.SingleOrDefault(i => string.Equals(i.RegistryConfig.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static T Referer<T>(this IApplicationModel model)
        {
            return model.Referer<T>(typeof(T).Name);
        }
    }

    public class ApplicationModelDescriptor
    {
        public ProtocolConfig[] Protocols { get; set; }
        public ServiceConfig[] Services { get; set; }
        public RefererConfig[] Referers { get; set; }
        public RegistryConfig[] Registrys { get; set; }
    }
}