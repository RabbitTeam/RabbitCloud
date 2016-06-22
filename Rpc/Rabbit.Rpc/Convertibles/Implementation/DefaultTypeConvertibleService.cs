using Rabbit.Rpc.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Rpc.Convertibles.Implementation
{
    /// <summary>
    /// 一个默认的类型转换服务。
    /// </summary>
    public class DefaultTypeConvertibleService : ITypeConvertibleService
    {
        #region Field

        private readonly IEnumerable<TypeConvertDelegate> _converters;

        #endregion Field

        #region Constructor

        public DefaultTypeConvertibleService(IEnumerable<ITypeConvertibleProvider> providers)
        {
            _converters = providers.SelectMany(p => p.GetConverters()).ToArray();
        }

        #endregion Constructor

        #region Implementation of ITypeConvertibleService

        /// <summary>
        /// 转换。
        /// </summary>
        /// <param name="instance">需要转换的实例。</param>
        /// <param name="conversionType">转换的类型。</param>
        /// <returns>转换之后的类型，如果无法转换则返回null。</returns>
        public object Convert(object instance, Type conversionType)
        {
            object result = null;
            foreach (var converter in _converters)
            {
                result = converter(instance, conversionType);
                if (result != null)
                    break;
            }
            if (result == null)
                throw new RpcException($"无法将实例：{instance}转换为{conversionType}。");
            return result;
        }

        #endregion Implementation of ITypeConvertibleService
    }
}