using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Binder;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Go.Internal
{
    public class DefaultParameterBinder : IParameterBinder
    {
        public static IParameterBinder Instance { get; } = new DefaultParameterBinder();

        #region Implementation of IParameterBinder

        public Task BindAsync(ParameterBindContext context)
        {
            return BuildAsync(context.ModelName, context.Model, context);
        }

        #endregion Implementation of IParameterBinder

        private static async Task BuildAsync(string name, object value, ParameterBindContext context)
        {
            if (value == null)
                return;

            switch (context.Target)
            {
                case ParameterTarget.Query:
                case ParameterTarget.Header:
                case ParameterTarget.Path:
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
                            await BuildEnumerableAsync(name, (IEnumerable)value, context);
                        }
                        else
                        {
                            var firstName = context.RequestContext.Arguments.Count == 1 ? string.Empty : name;
                            await BuildModelAsync(firstName, value, context);
                        }
                    }
                    else
                    {
                        AppendValue(context, context.Target, name, value);
                    }
                    break;

                /*case ParameterTarget.Items:
                    context.RequestContext.AppendItems(name, value);
                    break;*/

                case ParameterTarget.Body:
                    context.RequestContext.SetBody(value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task BuildModelAsync(string name, object model, ParameterBindContext context)
        {
            var type = model.GetType();
            foreach (var property in type.GetProperties())
            {
                var propertyValue = property.GetValue(model);

                var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();

                var prefix = string.IsNullOrEmpty(name) ? null : name + ".";

                var chidName = displayNameAttribute?.DisplayName ?? property.Name;
                if (!string.IsNullOrEmpty(prefix))
                    chidName = prefix + chidName;

                var parameterBinder = property.GetTypeAttribute<IParameterBinder>();
                if (parameterBinder != null)
                {
                    var bindContext = new ParameterBindContext
                    {
                        Model = propertyValue,
                        ModelName = chidName,
                        RequestContext = context.RequestContext,
                        Target = context.Target,
                        Type = property.PropertyType
                    };
                    await parameterBinder.BindAsync(bindContext);
                }
                else
                {
                    await BuildAsync(chidName, propertyValue, context);
                }
            }
        }

        private static async Task BuildEnumerableAsync(string name, IEnumerable enumerable, ParameterBindContext context)
        {
            var enumerator = enumerable.GetEnumerator();
            var index = 0;
            while (enumerator.MoveNext())
            {
                await BuildAsync($"{name}[{index}]", enumerator.Current, context);
                index++;
            }
        }

        private static void AppendValue(ParameterBindContext context, ParameterTarget target, string name, object value)
        {
            var requestContext = context.RequestContext;

            StringValues GetStringValues()
            {
                return value == null ? StringValues.Empty : new StringValues(value.ToString());
            }
            switch (target)
            {
                case ParameterTarget.Query:
                    requestContext.AddQuery(name, GetStringValues());
                    break;

                case ParameterTarget.Header:
                    requestContext.AddHeader(name, GetStringValues());
                    break;

/*                case ParameterTarget.Items:
                    requestContext.AppendItems(name, value);
                    break;*/

                case ParameterTarget.Path:
                    requestContext.AddPathVariable(name, GetStringValues());
                    break;

                case ParameterTarget.Body:
                    requestContext.SetBody(value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
}