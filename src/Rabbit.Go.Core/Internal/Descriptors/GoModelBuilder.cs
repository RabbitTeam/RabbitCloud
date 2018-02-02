using Rabbit.Go.Codec;
using Rabbit.Go.Core.Codec;
using Rabbit.Go.Core.GoModels;
using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Go.Core.Internal.Descriptors
{
    public static class GoModelBuilder
    {
        public static GoModel Build(IEnumerable<Type> types)
        {
            var goModel = new GoModel();

            Build(goModel, types);

            return goModel;
        }

        public static void Build(GoModel goModel, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var model = CreateModel(type);
                model.Go = goModel;
                goModel.Types.Add(model);
            }
        }

        private static TypeModel CreateModel(Type type)
        {
            var attributes = type.GetCustomAttributes(true);
            var model = new TypeModel(type, attributes);

            foreach (var interceptor in attributes.OfType<IInterceptorMetadata>())
                model.Interceptors.Add(interceptor);
            foreach (var methodModel in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(m => CreateModel(m, model)))
            {
                methodModel.Type = model;
                model.Methods.Add(methodModel);
            }

            return model;
        }

        private static MethodModel CreateModel(MethodInfo methodInfo, TypeModel typeModel)
        {
            var attributes = methodInfo.GetCustomAttributes(true);
            var model = new MethodModel(methodInfo, attributes)
            {
                Codec = typeModel.Attributes.Concat(attributes).OfType<ICodec>().LastOrDefault() ?? JsonCodec.Instance
            };

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