using System.Collections.Generic;

namespace Rabbit.Rpc.Server.Implementation
{
    /// <summary>
    /// 默认的服务条目管理者。
    /// </summary>
    public class DefaultServiceEntryManager : IServiceEntryManager
    {
        #region Field

        private readonly IEnumerable<ServiceEntry> _serviceEntries;

        #endregion Field

        #region Constructor

        public DefaultServiceEntryManager(IEnumerable<IServiceEntryProvider> providers)
        {
            var list = new List<ServiceEntry>();
            foreach (var provider in providers)
            {
                list.AddRange(provider.GetEntries());
            }
            _serviceEntries = list.ToArray();
        }

        #endregion Constructor

        #region Implementation of IServiceEntryManager

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<ServiceEntry> GetEntries()
        {
            return _serviceEntries;
        }

        #endregion Implementation of IServiceEntryManager
    }
}