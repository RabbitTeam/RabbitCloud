using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Convertibles;
using Rabbit.Rpc.Convertibles.Implementation;
using Rabbit.Rpc.Ids;
using Rabbit.Rpc.Ids.Implementation;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Routing.Implementation;
using Rabbit.Rpc.Runtime.Client;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Rabbit.Rpc.Runtime.Client.Implementation;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Runtime.Server.Implementation;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Implementation;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Serialization.Implementation;
using System;
using System.Linq;

namespace Rabbit.Rpc
{
    /// <summary>
    /// 一个抽象的Rpc服务构建者。
    /// </summary>
    public interface IRpcBuilder
    {
        /// <summary>
        /// 服务集合。
        /// </summary>
        IServiceCollection Services { get; }
    }

    /// <summary>
    /// 默认的Rpc服务构建者。
    /// </summary>
    internal sealed class RpcBuilder : IRpcBuilder
    {
        public RpcBuilder(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            Services = services;
        }

        #region Implementation of IRpcBuilder

        /// <summary>
        /// 服务集合。
        /// </summary>
        public IServiceCollection Services { get; }

        #endregion Implementation of IRpcBuilder
    }

    public static class RpcServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Json序列化支持。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder AddJsonSerialization(this IRpcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<ISerializer<string>, JsonSerializer>();
            services.AddSingleton<ISerializer<byte[]>, StringByteArraySerializer>();
            services.AddSingleton<ISerializer<object>, StringObjectSerializer>();

            return builder;
        }

        #region RouteManager

        /// <summary>
        /// 设置服务路由管理者。
        /// </summary>
        /// <typeparam name="T">服务路由管理者实现。</typeparam>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder SetRouteManager<T>(this IRpcBuilder builder) where T : class, IServiceRouteManager
        {
            builder.Services.AddSingleton<IServiceRouteManager, T>();
            return builder;
        }

        /// <summary>
        /// 设置服务路由管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="factory">服务路由管理者实例工厂。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder SetRouteManager(this IRpcBuilder builder, Func<IServiceProvider, IServiceRouteManager> factory)
        {
            builder.Services.AddSingleton(factory);
            return builder;
        }

        /// <summary>
        /// 设置服务路由管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="instance">服务路由管理者实例。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder SetRouteManager(this IRpcBuilder builder, IServiceRouteManager instance)
        {
            builder.Services.AddSingleton(instance);
            return builder;
        }

        #endregion RouteManager

        /// <summary>
        /// 设置共享文件路由管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="filePath">文件路径。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder SetSharedFileRouteManager(this IRpcBuilder builder, string filePath)
        {
            return builder.SetRouteManager(provider => new SharedFileServiceRouteManager(filePath, provider.GetRequiredService<ISerializer<string>>(), provider.GetRequiredService<ILogger<SharedFileServiceRouteManager>>()));
        }

        #region AddressSelector

        /// <summary>
        /// 设置服务地址选择器。
        /// </summary>
        /// <typeparam name="T">地址选择器实现类型。</typeparam>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder SetAddressSelector<T>(this IRpcBuilder builder) where T : class, IAddressSelector
        {
            builder.Services.AddSingleton<IAddressSelector, T>();
            return builder;
        }

        /// <summary>
        /// 设置服务地址选择器。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="factory">服务地址选择器实例工厂。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder SetAddressSelector(this IRpcBuilder builder,
            Func<IServiceProvider, IAddressSelector> factory)
        {
            builder.Services.AddSingleton(factory);

            return builder;
        }

        /// <summary>
        /// 设置服务地址选择器。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="instance">地址选择器实例。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder SetAddressSelector(this IRpcBuilder builder, IAddressSelector instance)
        {
            builder.Services.AddSingleton(instance);

            return builder;
        }

        #endregion AddressSelector

        public static IRpcBuilder AddClientCore(this IRpcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IAddressResolver, DefaultAddressResolver>();
            services.AddSingleton<IRemoteInvokeService, RemoteInvokeService>();

            return builder;
        }

        public static IRpcBuilder AddServerCore(this IRpcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IServiceInstanceFactory, DefaultServiceInstanceFactory>();
            services.AddSingleton<IClrServiceEntryFactory, ClrServiceEntryFactory>();
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetExportedTypes());
            services.AddSingleton<IServiceEntryProvider>(provider => new AttributeServiceEntryProvider(types, provider.GetRequiredService<IClrServiceEntryFactory>(), provider.GetRequiredService<ILogger<AttributeServiceEntryProvider>>()));
            services.AddSingleton<IServiceEntryManager, DefaultServiceEntryManager>();
            services.AddSingleton<IServiceEntryLocate, DefaultServiceEntryLocate>();
            services.AddSingleton<IServiceExecutor, DefaultServiceExecutor>();

            return builder;
        }

        public static IRpcBuilder AddRpcCore(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var builder = new RpcBuilder(services);

            builder.AddJsonSerialization();

            services.AddSingleton<IServiceIdGenerator, DefaultServiceIdGenerator>();

            services.AddSingleton<ITypeConvertibleProvider, DefaultTypeConvertibleProvider>();
            services.AddSingleton<ITypeConvertibleService, DefaultTypeConvertibleService>();

            return builder;
        }
    }
}