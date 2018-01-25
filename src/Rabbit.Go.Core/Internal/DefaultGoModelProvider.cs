using Microsoft.Extensions.Options;
using Rabbit.Go.Core.GoModels;
using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Go.Core.Internal
{
    public class DefaultGoModelProvider : IGoModelProvider
    {
        private readonly IList<IInterceptorMetadata> _globalInterceptors;

        public DefaultGoModelProvider(IOptions<GoOptions> optionsAccessor)
        {
            _globalInterceptors = optionsAccessor.Value.Interceptors;
        }

        #region Implementation of IGoModelProvider

        public int Order => -1000;

        public void OnProvidersExecuting(GoModelProviderContext context)
        {
            var goModel = context.Result;
            foreach (var interceptor in _globalInterceptors)
                goModel.Interceptors.Add(interceptor);

            foreach (var type in context.Types)
            {
                var model = CreateModel(type);
                model.Go = goModel;
                goModel.Types.Add(model);
            }
        }

        public void OnProvidersExecuted(GoModelProviderContext context)
        {
        }

        #endregion Implementation of IGoModelProvider

        private static TypeModel CreateModel(Type type)
        {
            var attributes = type.GetCustomAttributes(true);
            var model = new TypeModel(type, attributes);

            foreach (var interceptor in attributes.OfType<IInterceptorMetadata>())
                model.Interceptors.Add(interceptor);
            foreach (var methodModel in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(CreateModel))
            {
                methodModel.Type = model;
                model.Methods.Add(methodModel);
            }

            return model;
        }

        private static MethodModel CreateModel(MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes(true);
            var model = new MethodModel(methodInfo, attributes);
            foreach (var interceptor in attributes.OfType<IInterceptorMetadata>())
                model.Interceptors.Add(interceptor);
            foreach (var parameterModel in methodInfo.GetParameters().Select(CreateModel))
            {
                parameterModel.Method = model;
                model.Parameters.Add(parameterModel);
            }

            return model;
        }

        private static ParameterModel CreateModel(ParameterInfo parameterInfo)
        {
            var attributes = parameterInfo.GetCustomAttributes(true);
            return new ParameterModel(parameterInfo, attributes)
            {
                ParameterName = parameterInfo.Name,
                Target = GetParameterTarget(attributes)
            };
        }

        private static ParameterTarget GetParameterTarget(IEnumerable<object> attributes)
        {
            return attributes.OfType<GoParameterAttribute>().SingleOrDefault()?.Target ?? ParameterTarget.Query;
        }
    }
}