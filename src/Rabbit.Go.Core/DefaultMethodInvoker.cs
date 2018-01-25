using Microsoft.Extensions.Primitives;
using Rabbit.Go.Abstractions.Codec;
using Rabbit.Go.Codec;
using Rabbit.Go.Core.Internal;
using Rabbit.Go.Formatters;
using Rabbit.Go.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Go.Core
{
    public class DefaultMethodInvoker : InterceptorMethodInvoker
    {
        private readonly HttpClient _httpClient;
        private readonly MethodDescriptor _methodDescriptor;
        private readonly ICodec _codec;
        private readonly IKeyValueFormatterFactory _keyValueFormatterFactory;
        private readonly ITemplateParser _templateParser;

        public DefaultMethodInvoker(MethodDescriptor methodDescriptor, MethodInvokerEntry entry)
            : base(new RequestMessageBuilder(new UrlDescriptor(methodDescriptor.UrlTemplate.Template)), entry.Interceptors)
        {
            _httpClient = entry.Client;
            _methodDescriptor = methodDescriptor;
            _codec = entry.Codec;
            _keyValueFormatterFactory = entry.KeyValueFormatterFactory;
            _templateParser = entry.TemplateParser;
        }

        private static void BuildQueryAndHeaders(RequestMessageBuilder requestBuilder, IDictionary<ParameterTarget, IDictionary<string, StringValues>> parameters)
        {
            if (parameters == null)
                return;
            foreach (var item in parameters)
            {
                var target = item.Value;
                Func<string, StringValues, RequestMessageBuilder> set;
                switch (item.Key)
                {
                    case ParameterTarget.Query:
                        set = requestBuilder.Query;
                        break;

                    case ParameterTarget.Header:
                        set = requestBuilder.Header;
                        break;

                    default:
                        continue;
                }

                foreach (var t in target)
                {
                    set(t.Key, t.Value);
                }
            }
        }

        private static async Task BuildBodyAsync(RequestMessageBuilder requestBuilder, IEncoder encoder, IReadOnlyList<ParameterDescriptor> parameterDescriptors, object[] arguments)
        {
            if (encoder == null)
                return;

            object bodyArgument = null;
            Type bodyType = null;
            for (var i = 0; i < parameterDescriptors.Count; i++)
            {
                var parameterDescriptor = parameterDescriptors[i];
                if (parameterDescriptor.Target != ParameterTarget.Body)
                    continue;

                bodyArgument = arguments[i];
                bodyType = parameterDescriptor.ParameterType;
                break;
            }

            if (bodyArgument == null || bodyType == null)
                return;

            try
            {
                await encoder.EncodeAsync(bodyArgument, bodyType, requestBuilder);
            }
            catch (EncodeException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new EncodeException(e.Message, e);
            }
        }

        private static async Task<IDictionary<ParameterTarget, IDictionary<string, StringValues>>> FormatAsync(IReadOnlyList<ParameterDescriptor> parameterDescriptors, IKeyValueFormatterFactory keyValueFormatterFactory, IReadOnlyList<object> arguments)
        {
            if (keyValueFormatterFactory == null)
                return null;

            IDictionary<ParameterTarget, IDictionary<string, StringValues>> formatResult =
                new Dictionary<ParameterTarget, IDictionary<string, StringValues>>
                {
                    {
                        ParameterTarget.Query,
                        new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase)
                    },
                    {ParameterTarget.Path, new Dictionary<string, StringValues>()},
                    {
                        ParameterTarget.Header,
                        new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase)
                    }
                };

            for (var i = 0; i < parameterDescriptors.Count; i++)
            {
                var parameterDescriptor = parameterDescriptors[i];

                if (!formatResult.TryGetValue(parameterDescriptor.Target, out var itemResult))
                    continue;

                var parameter = parameterDescriptors[i];
                var value = arguments[i];
                var item = await keyValueFormatterFactory.FormatAsync(value, parameter.ParameterType, parameterDescriptor.Name);

                foreach (var t in item)
                    itemResult[t.Key] = t.Value;
            }

            return formatResult;
        }

        private async Task<object> DecodeAsync(HttpResponseMessage response)
        {
            try
            {
                return _codec?.Decoder == null
                    ? null
                    : await _codec?.Decoder.DecodeAsync(response, _methodDescriptor.ReturnType);
            }
            catch (DecodeException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new DecodeException(e.Message, e);
            }
        }

        #region Overrides of MethodInvokerBase

        protected override async Task<object> DoInvokeAsync(object[] arguments)
        {
            var formatResult = await FormatAsync(_methodDescriptor.Parameters, _keyValueFormatterFactory, arguments);

            var requestBuilder = RequestBuilder;
            var urlDescriptor = requestBuilder.UrlDescriptor;

            if (formatResult != null && _templateParser != null)
                urlDescriptor.Path = _templateParser.Parse(urlDescriptor.Path, formatResult[ParameterTarget.Path].ToDictionary(i => i.Key, i => i.Value.ToString()));

            var requestMessage = requestBuilder.Build();

            BuildQueryAndHeaders(requestBuilder, formatResult);
            await BuildBodyAsync(requestBuilder, _codec.Encoder, _methodDescriptor.Parameters, arguments);

            var responseMessage = await _httpClient.SendAsync(requestMessage);

            return await DecodeAsync(responseMessage);
        }

        #endregion Overrides of MethodInvokerBase
    }
}