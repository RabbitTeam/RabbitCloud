using System;
using System.Collections.Generic;

namespace Rabbit.Rpc.Logging.NLog
{
    /// <summary>
    /// 基于NLog的日志记录器。
    /// </summary>
    public class NLogLogger : ILogger
    {
        #region Field

        private readonly global::NLog.ILogger _nlogLogger;

        private static readonly Dictionary<LogLevel, global::NLog.LogLevel> LevelMappings = new Dictionary<LogLevel, global::NLog.LogLevel>()
        {
            {LogLevel.Trace, global::NLog.LogLevel.Trace },
            {LogLevel.Debug, global::NLog.LogLevel.Debug },
            {LogLevel.Information, global::NLog.LogLevel.Info},
            {LogLevel.Warning, global::NLog.LogLevel.Warn},
            {LogLevel.Error, global::NLog.LogLevel.Error},
            {LogLevel.Fatal, global::NLog.LogLevel.Fatal}
        };

        #endregion Field

        #region Constructor

        public NLogLogger(global::NLog.ILogger nlogLogger)
        {
            _nlogLogger = nlogLogger;
        }

        #endregion Constructor

        #region Implementation of ILogger

        /// <summary>
        /// 判断日志记录器是否开启。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <returns>如果开启返回true，否则返回false。</returns>
        public bool IsEnabled(LogLevel level)
        {
            return _nlogLogger.IsEnabled(ConvertLogLevel(level));
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
            _nlogLogger.Log(ConvertLogLevel(level), exception, message);
        }

        #endregion Implementation of ILogger

        #region Private Method

        private static global::NLog.LogLevel ConvertLogLevel(LogLevel level)
        {
            global::NLog.LogLevel logLevel;
            if (LevelMappings.TryGetValue(level, out logLevel))
                return logLevel;
            throw new NotSupportedException($"不支持的日志等级，{level}。");
        }

        #endregion Private Method
    }
}