using Rabbit.Go.Core.Codec;
using Rabbit.Go.Core.GoModels;
using Rabbit.Go.Interceptors;
using Rabbit.Go.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Go.Internal
{
    public class DefaultMethodDescriptorProvider : IMethodDescriptorProvider
    {
        private readonly IEnumerable<Type> _types;
        private readonly IReadOnlyList<IGoModelProvider> _modelProviders;

        public DefaultMethodDescriptorProvider(IEnumerable<Type> types, IEnumerable<IGoModelProvider> modelProviders)
        {
            _types = types;
            _modelProviders = modelProviders.OrderBy(i => i.Order).ToArray();
        }

        #region Implementation of IMethodDescriptorProvider

        public int Order { get; } = 0;

        public void OnProvidersExecuting(MethodDescriptorProviderContext context)
        {
            var providerContext = new GoModelProviderContext(_types);

            foreach (var provider in _modelProviders)
                provider.OnProvidersExecuting(providerContext);

            for (var i = _modelProviders.Count - 1; i >= 0; i--)
                _modelProviders[i].OnProvidersExecuted(providerContext);

            var model = providerContext.Result;

            foreach (var typeModel in model.Types)
            {
                var goAttribute = typeModel.Attributes.OfType<GoAttribute>().Single();
                foreach (var methodModel in typeModel.Methods)
                {
                    var goRequestAttribute = methodModel.Attributes.OfType<GoRequestAttribute>().Single();

                    var interceptorDescriptors = model
                        .Interceptors
                        .Concat(typeModel.Interceptors)
                        .Concat(methodModel.Interceptors)
                        .Select(i => new InterceptorDescriptor(i))
                        .OrderBy(i => i.Order)
                        .ToArray();

                    var baseUrl = goAttribute.Url;
                    var path = goRequestAttribute.Path;

                    var uri = baseUrl + path;

                    var returnType = methodModel.Method.ReturnType;

                    if (returnType.IsGenericType && typeof(Task).IsAssignableFrom(returnType))
                        returnType = returnType.GenericTypeArguments[0];

                    var methodDescriptor = new MethodDescriptor
                    {
                        ClienType = typeModel.Type,
                        Codec = JsonCodec.Instance,
                        InterceptorDescriptors = interceptorDescriptors,
                        Method = goRequestAttribute.Method,
                        MethodInfo = methodModel.Method,
                        ReturnType = returnType,
                        UrlTemplate = new TemplateString(uri, TemplateUtilities.GetVariables(uri))
                    };

                    var parameterModels = new List<ParameterDescriptor>(methodModel.Parameters.Count);
                    foreach (var parameterModel in methodModel.Parameters)
                    {
                        var goParameterAttribute = parameterModel.Attributes.OfType<GoParameterAttribute>().SingleOrDefault();

                        var name = goParameterAttribute?.Name ?? parameterModel.ParameterName;

                        var parameterDescriptor = new ParameterDescriptor
                        {
                            Name = goParameterAttribute?.Name ?? parameterModel.ParameterName,
                            ParameterType = parameterModel.ParameterInfo.ParameterType,
                            Target = GetParameterTarget(methodDescriptor.UrlTemplate, goParameterAttribute, name, parameterModel)
                        };
                        parameterModels.Add(parameterDescriptor);
                    }

                    methodDescriptor.Parameters = parameterModels;

                    context.Results.Add(methodDescriptor);
                }
            }
        }

        private static ParameterTarget GetParameterTarget(TemplateString urlTemplate, GoParameterAttribute goParameterAttribute, string name, ParameterModel parameterModel)
        {
            if (goParameterAttribute != null)
                return goParameterAttribute.Target;

            if (urlTemplate.Variables.Contains(name, StringComparer.OrdinalIgnoreCase))
                return ParameterTarget.Path;

            return parameterModel.Target;
        }

        public void OnProvidersExecuted(MethodDescriptorProviderContext context)
        {
        }

        #endregion Implementation of IMethodDescriptorProvider
    }
}