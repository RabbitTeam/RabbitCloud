using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Rabbit.Go.Codec;
using Rabbit.Go.Core.Internal;
using Rabbit.Go.Formatters;
using Rabbit.Go.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Go.Core
{
    public class DefaultMethodInvoker : InterceptorMethodInvoker
    {
        private readonly MethodInvokerEntry _entry;
        private readonly ICodec _codec;
        private readonly IKeyValueFormatterFactory _keyValueFormatterFactory;
        private readonly ITemplateParser _templateParser;
        private readonly IGoClient _client;

        public DefaultMethodInvoker(RequestContext requestContext, MethodInvokerEntry entry)
            : base(requestContext, entry.Interceptors)
        {
            _entry = entry;
            _client = entry.Client;
            _codec = entry.Codec;
            _keyValueFormatterFactory = entry.KeyValueFormatterFactory;
            _templateParser = entry.TemplateParser;
        }

        #region Overrides of InterceptorMethodInvoker

        protected override async Task<object> DoInvokeAsync()
        {
            var goContext = RequestContext.GoContext;

            await _client.RequestAsync(goContext);

            return await DecodeAsync(goContext.Response);
        }

        #endregion Overrides of InterceptorMethodInvoker

        private static void BuildQueryAndHeaders(GoRequest request, IDictionary<ParameterTarget, IDictionary<string, StringValues>> parameters)
        {
            if (parameters == null || !parameters.Any())
                return;
            foreach (var item in parameters)
            {
                var target = item.Value;
                if (!target.Any())
                    continue;
                Func<string, StringValues, GoRequest> set;
                switch (item.Key)
                {
                    case ParameterTarget.Query:
                        set = request.AddQuery;
                        break;

                    case ParameterTarget.Header:
                        set = request.AddHeader;
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

        private static async Task BuildBodyAsync(GoRequest request, IEncoder encoder, IReadOnlyList<ParameterDescriptor> parameterDescriptors, IReadOnlyList<object> arguments)
        {
            if (encoder == null || parameterDescriptors == null || !parameterDescriptors.Any() || arguments == null || !arguments.Any())
                return;

            object bodyArgument = null;
            Type bodyType = null;
            for (var i = 0; i < parameterDescriptors.Count; i++)
            {
                var parameterDescriptor = parameterDescriptors[i];
                if (parameterDescriptor.FormattingInfo.Target != ParameterTarget.Body)
                    continue;

                bodyArgument = arguments[i];
                bodyType = parameterDescriptor.ParameterType;
                break;
            }

            if (bodyArgument == null || bodyType == null)
                return;

            try
            {
                await encoder.EncodeAsync(bodyArgument, bodyType, request);
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

        private async Task<IDictionary<ParameterTarget, IDictionary<string, StringValues>>> FormatAsync(IReadOnlyList<ParameterDescriptor> parameterDescriptors, IKeyValueFormatterFactory keyValueFormatterFactory, IReadOnlyList<object> arguments)
        {
            if (keyValueFormatterFactory == null || parameterDescriptors == null || !parameterDescriptors.Any() || arguments == null || !arguments.Any())
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

                if (!formatResult.TryGetValue(parameterDescriptor.FormattingInfo.Target, out var itemResult))
                    continue;

                IKeyValueFormatter formatter = null;
                if (parameterDescriptor.FormattingInfo.FormatterType != null)
                    formatter = (IKeyValueFormatter)ActivatorUtilities.GetServiceOrCreateInstance(RequestContext.GoContext.RequestServices, parameterDescriptor.FormattingInfo.FormatterType);

                if (formatter == null)
                    formatter = keyValueFormatterFactory.CreateFormatter(parameterDescriptor.ParameterType);

                var parameter = parameterDescriptors[i];
                var value = arguments[i];
                var item = await keyValueFormatterFactory.FormatAsync(formatter, value, parameter.ParameterType, parameterDescriptor.FormattingInfo.FormatterName);

                foreach (var t in item)
                    itemResult[t.Key] = t.Value;
            }

            return formatResult;
        }

        private async Task<object> DecodeAsync(GoResponse response)
        {
            try
            {
                return _codec?.Decoder == null
                    ? null
                    : await _codec?.Decoder.DecodeAsync(response, RequestContext.MethodDescriptor.ReturnType);
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

        protected override async Task InitializeRequestAsync(GoRequest request, IReadOnlyList<object> arguments)
        {
            var methodDescriptor = RequestContext.MethodDescriptor;

            var formatResult = await FormatAsync(methodDescriptor.Parameters, _keyValueFormatterFactory, arguments);

            var urlTemplate = _entry.UrlTemplate;

            request.Method = methodDescriptor.Method;
            request.Scheme = urlTemplate.Scheme;
            request.Host = urlTemplate.Host;
            request.Port = urlTemplate.Port;

            var path = urlTemplate.Path;

            // render path
            if (path.Contains("{") && path.Contains("}"))
                path = _templateParser.Parse(path, formatResult[ParameterTarget.Path].ToDictionary(i => i.Key, i => i.Value.ToString()));

            request.Path = path;

            if (urlTemplate.HasQuery())
            {
                var queryString = urlTemplate.QueryString;
                var query = QueryHelpers.ParseNullableQuery(queryString);
                if (query != null && query.Any())
                {
                    foreach (var item in query)
                        request.AddQuery(item.Key, item.Value);
                }
            }

            await BuildBodyAsync(request, _codec.Encoder, methodDescriptor.Parameters, arguments);
            BuildQueryAndHeaders(request, formatResult);
        }
    }
}