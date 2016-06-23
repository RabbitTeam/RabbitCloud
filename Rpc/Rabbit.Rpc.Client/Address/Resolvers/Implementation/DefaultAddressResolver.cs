using Rabbit.Rpc.Address;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Routing;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Client.Address.Resolvers.Implementation
{
    /// <summary>
    /// 一个人默认的服务地址解析器。
    /// </summary>
    public class DefaultAddressResolver : IAddressResolver
    {
        #region Field

        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly ILogger<DefaultAddressResolver> _logger;

        #endregion Field

        #region Constructor

        public DefaultAddressResolver(IServiceRouteManager serviceRouteManager, ILogger<DefaultAddressResolver> logger)
        {
            _serviceRouteManager = serviceRouteManager;
            _logger = logger;
        }

        #endregion Constructor

        #region Implementation of IAddressResolver

        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>服务地址模型。</returns>
        public async Task<AddressModel> Resolver(string serviceId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Debug($"准备为服务id：{serviceId}，解析可用地址。");
            var descriptors = await _serviceRouteManager.GetRoutesAsync();
            var descriptor = descriptors.FirstOrDefault(i => i.ServiceDescriptor.Id == serviceId);

            if (descriptor == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.Warning($"根据服务id：{serviceId}，找不到相关服务信息。");
                return null;
            }

            var hasAddress = descriptor.Address?.Any();
            if (!hasAddress.HasValue || !hasAddress.Value)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.Warning($"根据服务id：{serviceId}，找不到可用的地址。");
                return null;
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Information($"根据服务id：{serviceId}，找到以下可用地址：{string.Join(",", descriptor.Address.Select(i => i.ToString()))}。");

            return descriptor.Address?.FirstOrDefault();
        }

        #endregion Implementation of IAddressResolver
    }
}