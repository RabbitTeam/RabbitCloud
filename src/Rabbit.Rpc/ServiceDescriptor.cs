using System;
using System.Collections.Generic;

namespace Rabbit.Rpc
{
    /// <summary>
    /// 服务描述符扩展方法。
    /// </summary>
    public static class ServiceDescriptorExtensions
    {
        /// <summary>
        /// 获取组名称。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        /// <returns>组名称。</returns>
        public static string GetGroupName(this ServiceDescriptor descriptor)
        {
            return descriptor.GetMetadata<string>("groupName");
        }

        /// <summary>
        /// 设置组名称。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        /// <param name="groupName">组名称。</param>
        /// <returns>服务描述符。</returns>
        public static ServiceDescriptor SetGroupName(this ServiceDescriptor descriptor, string groupName)
        {
            descriptor.Metadatas["groupName"] = groupName;

            return descriptor;
        }
    }

    /// <summary>
    /// 服务描述符。
    /// </summary>
    public class ServiceDescriptor
    {
        /// <summary>
        /// 初始化一个新的服务描述符。
        /// </summary>
        public ServiceDescriptor()
        {
            Metadatas = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 服务Id。
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 元数据。
        /// </summary>
        public IDictionary<string, object> Metadatas { get; set; }

        /// <summary>
        /// 获取一个元数据。
        /// </summary>
        /// <typeparam name="T">元数据类型。</typeparam>
        /// <param name="name">元数据名称。</param>
        /// <param name="def">如果指定名称的元数据不存在则返回这个参数。</param>
        /// <returns>元数据值。</returns>
        public T GetMetadata<T>(string name, T def = default(T))
        {
            if (!Metadatas.ContainsKey(name))
                return def;

            return (T)Metadatas[name];
        }
    }
}