using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Go.Abstractions;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go.Internal
{
    internal static class ParameterUtil
    {
        private struct GoParameterContext
        {
            public GoParameterContext(ParameterModel parameterModel, object argument)
            {
                ParameterModel = parameterModel;
                Argument = argument;
                OnlyOneParameter = parameterModel.Request.Parameters.Count == 1;
                Values = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            }

            public ParameterModel ParameterModel { get; }
            public IDictionary<string, StringValues> Values { get; }
            public bool OnlyOneParameter { get; }
            public object Argument { get; }

            public void AppendValue(string name, string value)
            {
                Values[name] = value;
            }
        }

        public static void BuildParameters(GoRequestContext goRequestContext, RequestModel requestModel)
        {
            var arguments = goRequestContext.Arguments;
            var parameterModels = requestModel.Parameters;

            foreach (var parameterModel in parameterModels)
            {
                var key = parameterModel.ParameterName;
                var argument = arguments[parameterModel.ParameterInfo.Name];

                IDictionary<string, StringValues> GetSimpleBuild()
                {
                    var context = new GoParameterContext(parameterModel, argument);
                    return GetSimpleValue(context);
                }
                switch (parameterModel.Target)
                {
                    case ParameterTarget.Query:
                        goRequestContext.AppendQuery(GetSimpleBuild());
                        break;

                    case ParameterTarget.Header:
                        goRequestContext.AppendHeaders(GetSimpleBuild());
                        break;

                    case ParameterTarget.Items:
                        goRequestContext.AppendItems(key, argument);
                        break;

                    case ParameterTarget.Path:
                        goRequestContext.AppendPathVariable(GetSimpleBuild());
                        break;

                    case ParameterTarget.Body:
                        goRequestContext.SetBody(argument);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static IDictionary<string, StringValues> GetSimpleValue(GoParameterContext context)
        {
            SimpleBuild(context.ParameterModel.ParameterName, context.Argument, context);
            return context.Values;
        }

        private static void SimpleBuild(string name, object value, GoParameterContext context)
        {
            if (value == null)
                return;

            var type = value.GetType();
            var code = Type.GetTypeCode(type);

            bool IsEnumerable()
            {
                return value is IEnumerable;
            }

            if (code == TypeCode.Object)
            {
                if (IsEnumerable())
                {
                    BuildEnumerable(name, (IEnumerable)value, context);
                }
                else
                {
                    BuildModel(context.OnlyOneParameter ? string.Empty : name, value, context);
                }
            }
            else
            {
                context.AppendValue(name, value.ToString());
            }
        }

        private static void BuildEnumerable(string name, IEnumerable enumerable, GoParameterContext context)
        {
            var enumerator = enumerable.GetEnumerator();
            var index = 0;
            while (enumerator.MoveNext())
            {
                SimpleBuild($"{name}[{index}]", enumerator.Current, context);
                index++;
            }
        }

        private static void BuildModel(string name, object model, GoParameterContext context)
        {
            var type = model.GetType();
            foreach (var property in type.GetProperties())
            {
                var propertyValue = property.GetValue(model);

                var chidName = string.IsNullOrEmpty(name) ? property.Name : name + "." + property.Name;

                SimpleBuild(chidName, propertyValue, context);
            }
        }
    }
}