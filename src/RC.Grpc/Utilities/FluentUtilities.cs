using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Grpc.Utilities
{
    public static class FluentUtilities
    {
        private static readonly ConcurrentDictionary<Type, Func<object, Task>> CallResulltWrappers = new ConcurrentDictionary<Type, Func<object, Task>>();

        public static Task WrapperCallResuleToTask(object callResult)
        {
            var type = callResult.GetType();

            if (CallResulltWrappers.TryGetValue(type, out var value))
                return value(callResult);

            var parameterExpression = Expression.Parameter(typeof(object));

            var convertExpression = Expression.Convert(parameterExpression, type);

            var responseAsyncPropertyExpression = Expression.Property(convertExpression, nameof(AsyncUnaryCall<object>.ResponseAsync));
            var func = Expression.Lambda<Func<object, Task>>(responseAsyncPropertyExpression, parameterExpression).Compile();

            CallResulltWrappers.TryAdd(type, func);

            return func(callResult);
        }
    }
}