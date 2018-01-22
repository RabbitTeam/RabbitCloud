using Rabbit.Go.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Go.Core.Utilities
{
    public static class MethodDescriptorUtilities
    {
        public static MethodDescriptor Create(Type type, MethodInfo method)
        {
            var goRequestAttribute = method.GetCustomAttribute<GoRequestAttribute>();

            var uri = GetUri(method.DeclaringType, method);
            var pathVariables = TemplateUtilities.GetVariables(uri);

            var returnType = method.ReturnType;
            if (returnType.IsGenericType && typeof(Task).IsAssignableFrom(returnType))
                returnType = returnType.GenericTypeArguments[0];

            var descriptor = new MethodDescriptor
            {
                ReturnType = returnType,
                Parameters = method.GetParameters().Select(p => Create(p, pathVariables)).ToArray(),
                Uri = GetUri(method.DeclaringType, method),
                Method = goRequestAttribute?.Method ?? "GET",
                MethodInfo = method,
                ClienType = type
            };

            descriptor.DefaultQuery
                .Merge(type.GetTypeAttributes<DefaultQueryAttribute>().ToDictionary(i => i.Name, i => i.Value))
                .Merge(method.GetTypeAttributes<DefaultQueryAttribute>().ToDictionary(i => i.Name, i => i.Value));

            descriptor.DefaultHeaders
                .Merge(type.GetTypeAttributes<DefaultHeaderAttribute>().ToDictionary(i => i.Name, i => i.Value))
                .Merge(method.GetTypeAttributes<DefaultHeaderAttribute>().ToDictionary(i => i.Name, i => i.Value));

            return descriptor;
        }

        private static ParameterDescriptor Create(ParameterInfo parameter, IEnumerable<string> pathVariables)
        {
            var goParameter = parameter.GetCustomAttributes().OfType<GoParameterAttribute>().FirstOrDefault();
            var name = goParameter?.Name ?? parameter.Name;

            ParameterTarget target;

            if (goParameter != null)
                target = goParameter.Target;
            else
                target = pathVariables.Contains(name) ? ParameterTarget.Path : ParameterTarget.Query;

            return new ParameterDescriptor
            {
                Name = name,
                Target = target,
                ParameterType = parameter.ParameterType
            };
        }

        private static string GetUri(Type type, MethodInfo method)
        {
            var baseUrl = type.GetCustomAttribute<GoAttribute>().Url;
            var path = method.GetCustomAttribute<GoRequestAttribute>().Path;

            if (string.IsNullOrEmpty(path))
                return baseUrl;

            if (!baseUrl.EndsWith("/") && !path.StartsWith("/"))
                path += "/";
            else if (baseUrl.EndsWith("/") && path.StartsWith("/"))
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

            return baseUrl + path;
        }
    }
}