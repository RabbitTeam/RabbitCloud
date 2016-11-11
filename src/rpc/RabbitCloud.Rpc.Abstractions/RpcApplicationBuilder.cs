using RabbitCloud.Rpc.Abstractions.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的Rpc应用程序构建器。
    /// </summary>
    public interface IRpcApplicationBuilder
    {
        /// <summary>
        /// 应用程序服务。
        /// </summary>
        IServiceProvider ApplicationServices { get; set; }

        /// <summary>
        /// 服务器特性。
        /// </summary>
        IRpcFeatureCollection ServerFeatures { get; }

        /// <summary>
        /// 属性字典。
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// 使用一个中间件。
        /// </summary>
        /// <param name="middleware">中间件委托。</param>
        /// <returns>Rpc应用程序构建器。</returns>
        IRpcApplicationBuilder Use(Func<RpcRequestDelegate, RpcRequestDelegate> middleware);

        /// <summary>
        /// 以当前构建器的状态创建一个新的Rpc应用程序构建器。
        /// </summary>
        /// <returns>Rpc应用程序构建器</returns>
        IRpcApplicationBuilder New();

        /// <summary>
        /// 构建一个Rpc请求委托。
        /// </summary>
        /// <returns>Rpc请求委托。</returns>
        RpcRequestDelegate Build();
    }

    public class RpcApplicationBuilder : IRpcApplicationBuilder
    {
        private readonly IList<Func<RpcRequestDelegate, RpcRequestDelegate>> _components = new List<Func<RpcRequestDelegate, RpcRequestDelegate>>();

        public RpcApplicationBuilder(IServiceProvider serviceProvider)
        {
            Properties = new Dictionary<string, object>();
            ApplicationServices = serviceProvider;
        }

        public RpcApplicationBuilder(IServiceProvider serviceProvider, object server)
            : this(serviceProvider)
        {
            SetProperty("ServerFeatures", server);
        }

        private RpcApplicationBuilder(IRpcApplicationBuilder builder)
        {
            Properties = builder.Properties;
        }

        #region Implementation of IRpcApplicationBuilder

        /// <summary>
        /// 应用程序服务。
        /// </summary>
        public IServiceProvider ApplicationServices
        {
            get
            {
                return GetProperty<IServiceProvider>("ApplicationServices");
            }
            set
            {
                SetProperty("ApplicationServices", value);
            }
        }

        /// <summary>
        /// 服务器特性。
        /// </summary>
        public IRpcFeatureCollection ServerFeatures => GetProperty<IRpcFeatureCollection>("ServerFeatures");

        /// <summary>
        /// 属性字典。
        /// </summary>
        public IDictionary<string, object> Properties { get; }

        /// <summary>
        /// 使用一个中间件。
        /// </summary>
        /// <param name="middleware">中间件委托。</param>
        /// <returns>Rpc应用程序构建器。</returns>
        public IRpcApplicationBuilder Use(Func<RpcRequestDelegate, RpcRequestDelegate> middleware)
        {
            _components.Add(middleware);
            return this;
        }

        /// <summary>
        /// 以当前构建器的状态创建一个新的Rpc应用程序构建器。
        /// </summary>
        /// <returns>Rpc应用程序构建器</returns>
        public IRpcApplicationBuilder New()
        {
            return new RpcApplicationBuilder(this);
        }

        /// <summary>
        /// 构建一个Rpc请求委托。
        /// </summary>
        /// <returns>Rpc请求委托。</returns>
        public RpcRequestDelegate Build()
        {
            RpcRequestDelegate app = context => Task.CompletedTask;

            foreach (var component in _components.Reverse())
            {
                app = component(app);
            }
            return app;
        }

        #endregion Implementation of IRpcApplicationBuilder

        #region Private Method

        private T GetProperty<T>(string key)
        {
            object value;
            return Properties.TryGetValue(key, out value) ? (T)value : default(T);
        }

        private void SetProperty<T>(string key, T value)
        {
            Properties[key] = value;
        }

        #endregion Private Method
    }
}