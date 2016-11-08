using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的返回结果。
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// 值。
        /// </summary>
        object Value { get; }

        /// <summary>
        /// 异常。
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// 属性。
        /// </summary>
        AttributeDictionary Attributes { get; set; }
    }

    /// <summary>
    /// 返回结果扩展方法。
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// 重建返回值，如果有异常则抛出。
        /// </summary>
        /// <param name="result">返回结果。</param>
        /// <returns>返回结果的返回值。</returns>
        public static object Recreate(this IResult result)
        {
            if (result.HasException())
                throw result.Exception;
            return result.Value;
        }

        /// <summary>
        /// 是否存在异常。
        /// </summary>
        /// <param name="result">返回结果。</param>
        /// <returns>如果有异常则返回true，否则返回false。</returns>
        public static bool HasException(this IResult result)
        {
            return result?.Exception != null;
        }
    }

    /// <summary>
    /// Rpc结果。
    /// </summary>
    public class RpcResult : IResult
    {
        #region Constructor

        public RpcResult()
        {
        }

        public RpcResult(object value)
        {
            Value = value;
        }

        public RpcResult(Exception exception)
        {
            Exception = exception;
        }

        #endregion Constructor

        #region Implementation of IResult

        /// <summary>
        /// 值。
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 异常。
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 属性。
        /// </summary>
        public AttributeDictionary Attributes { get; set; }

        #endregion Implementation of IResult
    }
}