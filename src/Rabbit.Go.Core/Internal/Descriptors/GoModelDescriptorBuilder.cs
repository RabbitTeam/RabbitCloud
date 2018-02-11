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
                var goAttribute = typeModel.Attributes.OfType<GoAttribute>().SingleOrDefault();
                if (goAttribute == null)
                    continue;
                foreach (var methodModel in typeModel.Methods)
                {
                    var goRequestAttribute = methodModel.Attributes.OfType<GoRequestAttribute>().SingleOrDefault();
                    if (goRequestAttribute == null)
                        continue;

                    var interceptorDescriptors = model
                        .Interceptors
                        .Concat(typeModel.Interceptors)
                        .Concat(methodModel.Interceptors)
                        .Select(i => new InterceptorDescriptor(i))
                        .OrderBy(i => i.Order)
                        .ToArray();

                    var baseUrl = goAttribute.Url;
                    var path = goRequestAttribute.Path;

                    if (baseUrl.EndsWith("/") && path.StartsWith("/"))
                        baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);
                    if (!baseUrl.EndsWith("/") && !path.StartsWith("/"))
                        path = path.Insert(0, "/");

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
                        var parameterDescriptor = new ParameterDescriptor
                        {
                            Name = parameterModel.ParameterName,
                            ParameterType = parameterModel.ParameterInfo.ParameterType,
                            FormattingInfo = new ParameterFormattingInfo
                            {
                                FormatterName = GetFormatterName(parameterModel),
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

        private static string GetFormatterName(ParameterModel parameter)
        {
            var goParameterAttribute = parameter.Attributes.OfType<GoParameterAttribute>().SingleOrDefault();

            // 如果 attribute name有效，则无条件使用 attribute 提供的 name
            if (goParameterAttribute?.Name != null)
                return goParameterAttribute.Name;

            // 如果对应目标只有一个参数则name为null，否则使用原参数名称
            return parameter.Method.Parameters.GroupBy(i => i.Target).Count() == 1 ? null : parameter.ParameterName;
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