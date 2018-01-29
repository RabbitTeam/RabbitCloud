using Microsoft.Extensions.Options;
using Rabbit.Go.Abstractions.Codec;
using Rabbit.Go.Core;
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
        private readonly IList<IGoModelConvention> _conventions;

        public DefaultMethodDescriptorProvider(
            IEnumerable<IGoModelProvider> modelProviders,
            IOptions<GoOptions> optionsAccessor)
        {
            var options = optionsAccessor.Value;
            _types = options.Types;
            _modelProviders = modelProviders.OrderBy(i => i.Order).ToArray();
            _conventions = options.Conventions;
        }

        #region Implementation of IMethodDescriptorProvider

        public int Order { get; } = 0;

        public void OnProvidersExecuting(MethodDescriptorProviderContext context)
        {
            var model = BuildModel();

            ApplyConventions(model);

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

                    var codec = methodModel.Attributes.Concat(typeModel.Attributes).OfType<ICodec>().FirstOrDefault() ??
                                JsonCodec.Instance;

                    var methodDescriptor = new MethodDescriptor
                    {
                        ClienType = typeModel.Type,
                        Codec = codec,
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
                            Name = name,
                            ParameterType = parameterModel.ParameterInfo.ParameterType,
                            Target = GetParameterTarget(methodDescriptor, name, parameterModel)
                        };

                        parameterModels.Add(parameterDescriptor);
                    }

                    methodDescriptor.Parameters = parameterModels;

                    context.Results.Add(methodDescriptor);
                }
            }
        }

        public void OnProvidersExecuted(MethodDescriptorProviderContext context)
        {
        }

        #endregion Implementation of IMethodDescriptorProvider

        #region Private Method

        private void ApplyConventions(GoModel model)
        {
            foreach (var convention in _conventions)
                convention.Apply(model);

            foreach (var type in model.Types)
            {
                foreach (var typeModelConvention in type.Attributes.OfType<ITypeModelConvention>())
                    typeModelConvention.Apply(type);

                foreach (var methodModel in type.Methods)
                {
                    foreach (var methodModelConvention in methodModel.Attributes.OfType<IMethodModelConvention>())
                        methodModelConvention.Apply(methodModel);

                    foreach (var parameterModel in methodModel.Parameters)
                        foreach (var parameterModelConvention in parameterModel.Attributes.OfType<IParameterModelConvention>())
                            parameterModelConvention.Apply(parameterModel);
                }
            }
        }

        private GoModel BuildModel()
        {
            var providerContext = new GoModelProviderContext(_types);

            foreach (var provider in _modelProviders)
                provider.OnProvidersExecuting(providerContext);

            for (var i = _modelProviders.Count - 1; i >= 0; i--)
                _modelProviders[i].OnProvidersExecuted(providerContext);

            return providerContext.Result;
        }

        private static ParameterTarget GetParameterTarget(MethodDescriptor methodDescriptor, string name, ParameterModel parameterModel)
        {
            var goParameterAttribute = parameterModel.Attributes.OfType<GoParameterAttribute>().SingleOrDefault();

            if (goParameterAttribute != null)
                return goParameterAttribute.Target;

            var urlVariables = methodDescriptor.UrlTemplate.Variables;
            // is path variable
            if (urlVariables.Contains(name, StringComparer.OrdinalIgnoreCase))
                return ParameterTarget.Path;

            return parameterModel.Target;
        }

        #endregion Private Method
    }
}