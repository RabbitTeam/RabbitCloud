using RabbitCloud.Rpc.Abstractions.Features;
using System;
using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的Rpc请求。
    /// </summary>
    public abstract class RpcRequest
    {
        /// <summary>
        /// Rpc上下文。
        /// </summary>
        public abstract RpcContext RpcContext { get; }

        /// <summary>
        /// 服务Id。
        /// </summary>
        public abstract string ServiceId { get; set; }

        /// <summary>
        /// 请求主体。
        /// </summary>
        public abstract object Body { get; set; }
    }

    /// <summary>
    /// 一个抽象的Rpc响应。
    /// </summary>
    public abstract class RpcResponse
    {
        /// <summary>
        /// Rpc上下文。
        /// </summary>
        public abstract RpcContext RpcContext { get; }

        /// <summary>
        /// 响应主体。
        /// </summary>
        public abstract object Body { get; set; }
    }

    /// <summary>
    /// 一个抽象的Rpc上下文。
    /// </summary>
    public abstract class RpcContext
    {
        /// <summary>
        /// 上下文特性集合。
        /// </summary>
        public abstract IRpcFeatureCollection Features { get; }

        /// <summary>
        /// 获取这个Rpc请求。
        /// </summary>
        public abstract RpcRequest Request { get; }

        /// <summary>
        /// 获取这个Rpc响应。
        /// </summary>
        public abstract RpcResponse Response { get; }

        /// <summary>
        /// 获取请求的连接信息。
        /// </summary>
        public abstract ConnectionInfo Connection { get; }

        /// <summary>
        /// 获取或设置请求范围内的键值集合。
        /// </summary>
        public abstract IDictionary<object, object> Items { get; set; }

        /// <summary>
        /// 获取或设置请求的服务容器。
        /// </summary>
        public abstract IServiceProvider RequestServices { get; set; }
    }

    public class DefaultRpcRequest : RpcRequest
    {
        private readonly IRpcRequestFeature _requestFeature;

        public DefaultRpcRequest(RpcContext context)
        {
            RpcContext = context;
            var features = context.Features;
            _requestFeature = features.Get<IRpcRequestFeature>();
        }

        #region Overrides of RpcRequest

        /// <summary>
        /// Rpc上下文。
        /// </summary>
        public override RpcContext RpcContext { get; }

        /// <summary>
        /// 路径。
        /// </summary>
        public override string ServiceId
        {
            get { return _requestFeature.ServiceId; }
            set { _requestFeature.ServiceId = value; }
        }

        /// <summary>
        /// 请求主体。
        /// </summary>
        public override object Body
        {
            get { return _requestFeature.Body; }
            set { _requestFeature.Body = value; }
        }

        #endregion Overrides of RpcRequest
    }

    public class DefaultRpcResponse : RpcResponse
    {
        private readonly IRpcResponseFeature _responseFeature;

        public DefaultRpcResponse(RpcContext rpcContext)
        {
            RpcContext = rpcContext;
            _responseFeature = rpcContext.Features.Get<IRpcResponseFeature>();
        }

        #region Overrides of RpcResponse

        /// <summary>
        /// Rpc上下文。
        /// </summary>
        public override RpcContext RpcContext { get; }

        /// <summary>
        /// 响应主体。
        /// </summary>
        public override object Body
        {
            get { return _responseFeature.Body; }
            set { _responseFeature.Body = value; }
        }

        #endregion Overrides of RpcResponse
    }

    public class DefaultRpcContext : RpcContext
    {
        private readonly IItemsFeature _itemsFeature;
        private readonly IServiceProvidersFeature _serviceProvidersFeature;

        public DefaultRpcContext() : this(new RpcFeatureCollection())
        {
        }

        public DefaultRpcContext(IRpcFeatureCollection features)
        {
            Features = features;
            Request = new DefaultRpcRequest(this);
            Response = new DefaultRpcResponse(this);
            Connection = new DefaultConnectionInfo(features);
            _itemsFeature = new ItemsFeature();
            _serviceProvidersFeature = new ServiceProvidersFeature();
        }

        #region Overrides of RpcContext

        /// <summary>
        /// 上下文特性集合。
        /// </summary>
        public override IRpcFeatureCollection Features { get; }

        /// <summary>
        /// 获取这个Rpc请求。
        /// </summary>
        public override RpcRequest Request { get; }

        /// <summary>
        /// 获取这个Rpc响应。
        /// </summary>
        public override RpcResponse Response { get; }

        /// <summary>
        /// 获取请求的连接信息。
        /// </summary>
        public override ConnectionInfo Connection { get; }

        /// <summary>
        /// 获取或设置请求范围内的键值集合。
        /// </summary>
        public override IDictionary<object, object> Items
        {
            get { return _itemsFeature.Items; }
            set { _itemsFeature.Items = value; }
        }

        /// <summary>
        /// 获取或设置请求的服务容器。
        /// </summary>
        public override IServiceProvider RequestServices
        {
            get { return _serviceProvidersFeature.RequestServices; }
            set { _serviceProvidersFeature.RequestServices = value; }
        }

        #endregion Overrides of RpcContext
    }
}