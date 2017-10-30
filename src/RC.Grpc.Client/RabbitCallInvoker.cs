using Grpc.Core;
using System;

namespace Rabbit.Cloud.Grpc.Client
{
    public class RabbitCallInvoker : CallInvoker
    {
        private readonly IChannelLocator _channelLocator;

        public RabbitCallInvoker(IChannelLocator channelLocator)
        {
            _channelLocator = channelLocator ?? throw new ArgumentNullException(nameof(channelLocator));
        }

        #region Overrides of CallInvoker

        /// <inheritdoc />
        /// <summary>
        /// Invokes a simple remote call in a blocking fashion.
        /// </summary>
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var call = CreateCall(method, host, options);
            return Calls.BlockingUnaryCall(call, request);
        }

        /// <inheritdoc />
        /// <summary>
        /// Invokes a simple remote call asynchronously.
        /// </summary>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var call = CreateCall(method, host, options);
            return Calls.AsyncUnaryCall(call, request);
        }

        /// <inheritdoc />
        /// <summary>
        /// Invokes a server streaming call asynchronously.
        /// In server streaming scenario, client sends on request and server responds with a stream of responses.
        /// </summary>
        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options,
            TRequest request)
        {
            var call = CreateCall(method, host, options);
            return Calls.AsyncServerStreamingCall(call, request);
        }

        /// <inheritdoc />
        /// <summary>
        /// Invokes a client streaming call asynchronously.
        /// In client streaming scenario, client sends a stream of requests and server responds with a single response.
        /// </summary>
        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            var call = CreateCall(method, host, options);
            return Calls.AsyncClientStreamingCall(call);
        }

        /// <inheritdoc />
        /// <summary>
        /// Invokes a duplex streaming call asynchronously.
        /// In duplex streaming scenario, client sends a stream of requests and server responds with a stream of responses.
        /// The response stream is completely independent and both side can be sending messages at the same time.
        /// </summary>
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            var call = CreateCall(method, host, options);
            return Calls.AsyncDuplexStreamingCall(call);
        }

        #endregion Overrides of CallInvoker

        /// <summary>
        /// Creates call invocation details for given method.
        /// </summary>
        protected virtual CallInvocationDetails<TRequest, TResponse> CreateCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
            where TRequest : class
            where TResponse : class
        {
            var channel = _channelLocator.Locate(method.FullName);
            return new CallInvocationDetails<TRequest, TResponse>(channel, method, host, options);
        }
    }
}