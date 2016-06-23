using System;

namespace Rabbit.Rpc.Logging
{
    /// <summary>
    /// 一个空的日志记录器。
    /// </summary>
    public sealed class NullLogger<T> : NullLogger, ILogger<T>
    {
    }

    /// <summary>
    /// 一个空的日志记录器。
    /// </summary>
    public class NullLogger : ILogger
    {
        public static NullLogger Instance { get; } = new NullLogger();

        #region Implementation of ILogger

        /// <summary>
        /// 判断日志记录器是否开启。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <returns>如果开启返回true，否则返回false。</returns>
        public bool IsEnabled(LogLevel level)
        {
            return false;
        }

        /// <summary>
        /// 记录日志。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常。</param>
        /// <returns>任务。</returns>
        public void Log(LogLevel level, string message, Exception exception = null)
        {
            throw new NotImplementedException();
        }

        #endregion Implementation of ILogger
    }
}