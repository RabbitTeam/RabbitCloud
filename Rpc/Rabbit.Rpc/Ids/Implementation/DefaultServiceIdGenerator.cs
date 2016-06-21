using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Rpc.Ids.Implementation
{
    /// <summary>
    /// 一个默认的服务Id生成器。
    /// </summary>
    public class DefaultServiceIdGenerator : IServiceIdGenerator
    {
        #region Implementation of IServiceIdFactory

        /// <summary>
        /// 生成一个服务Id。
        /// </summary>
        /// <param name="method">本地方法信息。</param>
        /// <returns>对应方法的唯一服务Id。</returns>
        public string GenerateServiceId(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            var type = method.DeclaringType;
            if (type == null)
                throw new ArgumentNullException(nameof(method.DeclaringType), "方法的定义类型不能为空。");
            var id = $"{type.FullName}.{method.Name}";
            var parameters = method.GetParameters();
            if (parameters.Any())
            {
                id += "_" + string.Join("_", parameters.Select(i => i.Name));
            }
            return id;
        }

        #endregion Implementation of IServiceIdFactory
    }
}