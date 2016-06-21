using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Rpc.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// Service标记类型的服务条目提供程序。
    /// </summary>
    public class AttributeServiceEntryProvider : IServiceEntryProvider
    {
        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly IClrServiceEntryFactory _clrServiceEntryFactory;

        #endregion Field

        #region Constructor

        public AttributeServiceEntryProvider(IEnumerable<Type> types, IClrServiceEntryFactory clrServiceEntryFactory)
        {
            _types = types;
            _clrServiceEntryFactory = clrServiceEntryFactory;
        }

        #endregion Constructor

        #region Implementation of IServiceEntryProvider

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<ServiceEntry> GetEntries()
        {
            var services = _types.Where(i => i.IsInterface && i.GetCustomAttribute<RpcServiceAttribute>() != null);
            var serviceImplementations = _types.Where(i => i.IsClass && !i.IsAbstract).ToArray();

            var entries = new List<ServiceEntry>();
            foreach (var service in services)
            {
                foreach (var serviceImplementation in serviceImplementations.Where(i => service.IsAssignableFrom(i)))
                {
                    entries.AddRange(_clrServiceEntryFactory.CreateServiceEntry(service, serviceImplementation));
                }
            }
            return entries;
        }

        #endregion Implementation of IServiceEntryProvider
    }
}