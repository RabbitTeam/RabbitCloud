using System;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IResult
    {
        object Value { get; }
        Exception Exception { get; }
    }

    public static class ResultExtensions
    {
        public static object Recreate(this IResult result)
        {
            if (result.HasException())
                throw result.Exception;
            return result.Value;
        }

        public static bool HasException(this IResult result)
        {
            return result?.Exception != null;
        }
    }

    public class Result : IResult
    {
        public Result()
        {
        }

        public Result(object value)
        {
            Value = value;
        }

        public Result(Exception exception)
        {
            Exception = exception;
        }

        #region Implementation of IResult

        public object Value { get; }
        public Exception Exception { get; }

        #endregion Implementation of IResult
    }
}