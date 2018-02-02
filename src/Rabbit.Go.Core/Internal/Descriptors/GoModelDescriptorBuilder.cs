using Rabbit.Go.Core.GoModels;
using Rabbit.Go.Formatters;
using Rabbit.Go.Interceptors;
using Rabbit.Go.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Go.Core.Internal.Descriptors
{
    public static class GoModelDescriptorBuilder
    {
        public static IList<MethodDescriptor> Build(GoModel model)
        {
            var descriptors = new List<MethodDescriptor>();

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
                        Codec = methodModel.Codec,
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

                        var parameterDescriptor = new ParameterDescriptor
                        {
                            Name = parameterModel.ParameterName,
                            ParameterType = parameterModel.ParameterInfo.ParameterType,
                            FormattingInfo = new ParameterFormattingInfo
                            {
                                FormatterName = goParameterAttribute?.Name,
                                FormatterType = parameterModel.Attributes.OfType<CustomFormatterAttribute>().LastOrDefault()?.FormatterType,
                                Target = GetParameterTarget(methodDescriptor, parameterModel)
                            }
                        };

                        parameterModels.Add(parameterDescriptor);
                    }

                    methodDescriptor.Parameters = parameterModels;

                    descriptors.Add(methodDescriptor);
                }
            }

            return descriptors;
        }

        private static ParameterTarget GetParameterTarget(MethodDescriptor methodDescriptor, ParameterModel parameterModel)
        {
            var goParameterAttribute = parameterModel.Attributes.OfType<GoParameterAttribute>().SingleOrDefault();

            if (goParameterAttribute != null)
                return goParameterAttribute.Target;

            var urlVariables = methodDescriptor.UrlTemplate.Variables;
            // is path variable
            if (urlVariables.Contains(parameterModel.ParameterName, StringComparer.OrdinalIgnoreCase))
                return ParameterTarget.Path;

            return parameterModel.Target;
        }
    }
}