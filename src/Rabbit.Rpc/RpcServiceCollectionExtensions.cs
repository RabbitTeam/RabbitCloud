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
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Rabbit.Rpc.Runtime.Client.HealthChecks;
using Rabbit.Rpc.Runtime.Client.HealthChecks.Implementation;
using Rabbit.Rpc.Runtime.Client.Implementation;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Runtime.Server.Implementation;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Implementation;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Serialization.Implementation;
using Rabbit.Rpc.Transport.Codec;
using Rabbit.Rpc.Transport.Codec.Implementation;
using System;
using System.Linq;

#if !NET

using Microsoft.Extensions.DependencyModel;
using System.Reflection;

#endif

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
        public static IRpcBuilder UseRouteManager<T>(this IRpcBuilder builder) where T : class, IServiceRouteManager
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
        public static IRpcBuilder UseRouteManager(this IRpcBuilder builder, Func<IServiceProvider, IServiceRouteManager> factory)
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
        public static IRpcBuilder UseRouteManager(this IRpcBuilder builder, IServiceRouteManager instance)
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
        public static IRpcBuilder UseSharedFileRouteManager(this IRpcBuilder builder, string filePath)
        {
            return builder.UseRouteManager(provider =>
            new SharedFileServiceRouteManager(
                filePath,
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceRouteFactory>(),
                provider.GetRequiredService<ILogger<SharedFileServiceRouteManager>>()));
        }

        #region AddressSelector

        /// <summary>
        /// 设置服务地址选择器。
        /// </summary>
        /// <typeparam name="T">地址选择器实现类型。</typeparam>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder UseAddressSelector<T>(this IRpcBuilder builder) where T : class, IAddressSelector
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
        public static IRpcBuilder UseAddressSelector(this IRpcBuilder builder,
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
        public static IRpcBuilder UseAddressSelector(this IRpcBuilder builder, IAddressSelector instance)
        {
            builder.Services.AddSingleton(instance);

            return builder;
        }

        #endregion AddressSelector

        /// <summary>
        /// 使用轮询的地址选择器。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder UsePollingAddressSelector(this IRpcBuilder builder)
        {
            builder.Services.AddSingleton<IAddressSelector, PollingAddressSelector>();
            return builder;
        }

        /// <summary>
        /// 使用随机的地址选择器。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder UseRandomAddressSelector(this IRpcBuilder builder)
        {
            builder.Services.AddSingleton<IAddressSelector, RandomAddressSelector>();
            return builder;
        }

        #region Codec Factory

        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="codecFactory"></param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder UseCodec(this IRpcBuilder builder, ITransportMessageCodecFactory codecFactory)
        {
            builder.Services.AddSingleton(codecFactory);

            return builder;
        }

        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <typeparam name="T">编解码器工厂实现类型。</typeparam>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder UseCodec<T>(this IRpcBuilder builder) where T : class, ITransportMessageCodecFactory
        {
            builder.Services.AddSingleton<ITransportMessageCodecFactory, T>();

            return builder;
        }

        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="codecFactory">编解码器工厂创建委托。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder UseCodec(this IRpcBuilder builder, Func<IServiceProvider, ITransportMessageCodecFactory> codecFactory)
        {
            builder.Services.AddSingleton(codecFactory);

            return builder;
        }

        #endregion Codec Factory

        /// <summary>
        /// 使用Json编解码器。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder UseJsonCodec(this IRpcBuilder builder)
        {
            return builder.UseCodec<JsonTransportMessageCodecFactory>();
        }

        /// <summary>
        /// 添加客户端运行时服务。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder AddClientRuntime(this IRpcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IHealthCheckService, DefaultHealthCheckService>();
            services.AddSingleton<IAddressResolver, DefaultAddressResolver>();
            services.AddSingleton<IRemoteInvokeService, RemoteInvokeService>();

            return builder.UsePollingAddressSelector();
        }

        /// <summary>
        /// 添加服务运行时服务。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder AddServiceRuntime(this IRpcBuilder builder)
        {
            var services = builder.Services;

            //            services.AddSingleton<IServiceInstanceFactory, DefaultServiceInstanceFactory>();
            services.AddSingleton<IClrServiceEntryFactory, ClrServiceEntryFactory>();
            services.AddSingleton<IServiceEntryProvider>(provider =>
            {
#if NET
                var assemblys = AppDomain.CurrentDomain.GetAssemblies();
#else
                var assemblys = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default).Select(z => Assembly.Load(new AssemblyName(z.Name))));
#endif

                var types = assemblys.Where(i => i.IsDynamic == false).SelectMany(i => i.ExportedTypes).ToArray();

                return new AttributeServiceEntryProvider(types, provider.GetRequiredService<IClrServiceEntryFactory>(),
                    provider.GetRequiredService<ILogger<AttributeServiceEntryProvider>>());
            });
            services.AddSingleton<IServiceEntryManager, DefaultServiceEntryManager>();
            services.AddSingleton<IServiceEntryLocate, DefaultServiceEntryLocate>();
            services.AddSingleton<IServiceExecutor, DefaultServiceExecutor>();

            return builder;
        }

        /// <summary>
        /// 添加RPC核心服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder AddRpcCore(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IServiceIdGenerator, DefaultServiceIdGenerator>();

            services.AddSingleton<ITypeConvertibleProvider, DefaultTypeConvertibleProvider>();
            services.AddSingleton<ITypeConvertibleService, DefaultTypeConvertibleService>();

            services.AddSingleton<IServiceRouteFactory, DefaultServiceRouteFactory>();

            return new RpcBuilder(services)
                .AddJsonSerialization()
                .UseJsonCodec();
        }
    }
}