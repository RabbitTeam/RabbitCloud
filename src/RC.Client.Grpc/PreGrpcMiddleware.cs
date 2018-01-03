using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Codec;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using Rabbit.Cloud.Client.Serialization;
using Rabbit.Cloud.Grpc.Client;
using Rabbit.Cloud.Grpc.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc
{
    internal class GrpcCodec : ICodec
    {
        private readonly ICodec _codec;
        private readonly Type _requestType;
        private readonly Type _responseType;

        public GrpcCodec(ICodec codec, Type requestType, Type responseType)
        {
            _codec = codec;
            _requestType = requestType;
            _responseType = responseType;
        }

        #region Implementation of ICodec

        public object Encode(object body)
        {
            return typeof(IMessage).IsAssignableFrom(_requestType) ? GrpcProtobufSerializer.Instance.Serializer(body) : _codec.Encode(body);
        }

        public object Decode(object data)
        {
            return typeof(IMessage).IsAssignableFrom(_responseType) ? GrpcProtobufSerializer.Instance.Deserialize((byte[])data, _responseType) : _codec.Decode(data);
        }

        #endregion Implementation of ICodec
    }

    internal class GrpcProtobufSerializer : ISerializer
    {
        public static ISerializer Instance { get; } = new GrpcProtobufSerializer();

        #region Implementation of ISerializer

        public void Serialize(object instance, Stream stream)
        {
            if (instance is IMessage message)
            {
                message.WriteTo(stream);
            }
        }

        public object Deserialize(Stream stream, Type type)
        {
            if (Activator.CreateInstance(type) is IMessage message)
            {
                message.MergeFrom(stream);
                return message;
            }
            return null;
        }

        #endregion Implementation of ISerializer
    }

    public class PreGrpcMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ChannelPool _channelPool;

        private static readonly TimeSpan DefaultConnectionTimeout = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan DefaultReadTimeout = TimeSpan.FromSeconds(10);

        public PreGrpcMiddleware(RabbitRequestDelegate next, ChannelPool channelPool)
        {
            _next = next;
            _channelPool = channelPool;
        }

        private struct MarshallerCache
        {
            public MarshallerCache(object requestMarshaller, object responseMarshaller) : this()
            {
                RequestMarshaller = requestMarshaller;
                ResponseMarshaller = responseMarshaller;
            }

            public object RequestMarshaller { get; }
            public object ResponseMarshaller { get; }
        }

        private static readonly ConcurrentDictionary<(Type, Type), MarshallerCache> _marshallerCache = new ConcurrentDictionary<(Type, Type), MarshallerCache>();

        public async Task Invoke(IRabbitContext context)
        {
            var request = context.Request;
            var serviceRequestFeature = context.Features.Get<IServiceRequestFeature>();

            var grpcFeature = context.Features.Get<IGrpcFeature>();
            if (grpcFeature == null)
            {
                grpcFeature = new GrpcFeature();
                context.Features.Set(grpcFeature);
            }

            var serviceInstance = serviceRequestFeature.GetServiceInstance();
            var requestOptions = serviceRequestFeature.RequestOptions;

            var connectionTimeout = requestOptions?.ConnectionTimeout ?? DefaultConnectionTimeout;
            var readTimeout = requestOptions?.ReadTimeout ?? DefaultReadTimeout;

            if (grpcFeature.Channel == null)
            {
                var channel = _channelPool.GetChannel(serviceInstance.Host, serviceInstance.Port);
                await channel.ConnectAsync(DateTime.UtcNow.Add(connectionTimeout));
                grpcFeature.Channel = channel;
            }

            var codec = serviceRequestFeature.Codec;
            if (!(codec is GrpcCodec))
                serviceRequestFeature.Codec = codec = new GrpcCodec(serviceRequestFeature.Codec, serviceRequestFeature.RequesType, serviceRequestFeature.ResponseType);

            var entry = _marshallerCache.GetOrAdd((serviceRequestFeature.RequesType, serviceRequestFeature.ResponseType), key =>
            {
                var requestMarshaller = MarshallerUtilities.CreateGenericMarshaller(
                    serviceRequestFeature.RequesType,
                    instance => (byte[])codec.Encode(instance), data => codec.Decode(data));
                var responseMarshaller = MarshallerUtilities.CreateGenericMarshaller(
                    serviceRequestFeature.ResponseType,
                    instance => (byte[])codec.Encode(instance), data => codec.Decode(data));

                return new MarshallerCache(requestMarshaller, responseMarshaller);
            });

            grpcFeature.RequestMarshaller = entry.RequestMarshaller;
            grpcFeature.ResponseMarshaller = entry.ResponseMarshaller;

            var callOptionsNullable = grpcFeature.CallOptions;

            CallOptions callOptions;

            //追加选项
            if (callOptionsNullable.HasValue)
            {
                callOptions = callOptionsNullable.Value;

                if (!callOptions.Deadline.HasValue)
                    callOptions = callOptions.WithDeadline(DateTime.UtcNow.Add(readTimeout));

                var headers = callOptions.Headers ?? new Metadata();
                if (AppendHeaders(headers, request))
                    callOptions = callOptions.WithHeaders(headers);
            }
            else
            {
                var headers = new Metadata();
                if (!AppendHeaders(headers, request))
                    headers = null;
                callOptions = new CallOptions(headers, DateTime.UtcNow.Add(readTimeout));
            }

            grpcFeature.CallOptions = callOptions;

            await _next(context);
        }

        private static bool AppendHeaders(Metadata headers, IRabbitRequest request)
        {
            IDictionary<string, StringValues> appendHeaders = null;

            if (request.Query.Any() || request.Headers.Any())
            {
                appendHeaders = new Dictionary<string, StringValues>();
                foreach (var query in request.Query)
                {
                    StringValues values;
                    values = appendHeaders.TryGetValue(query.Key, out values) ? StringValues.Concat(values, query.Value) : query.Value;
                    appendHeaders[query.Key] = values;
                }
                foreach (var header in request.Headers)
                {
                    StringValues values;
                    values = appendHeaders.TryGetValue(header.Key, out values) ? StringValues.Concat(values, header.Value) : header.Value;
                    appendHeaders[header.Key] = values;
                }
            }

            if (appendHeaders == null || appendHeaders.Any())
                return false;

            foreach (var header in appendHeaders)
            {
                headers.Add(header.Key, header.Value.ToString());
            }
            return true;
        }
    }
}