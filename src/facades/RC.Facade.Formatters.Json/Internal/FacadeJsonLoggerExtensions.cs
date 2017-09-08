using Microsoft.Extensions.Logging;
using System;

namespace RC.Facade.Formatters.Json.Internal
{
    internal static class FacadeJsonLoggerExtensions
    {
        private static readonly Action<ILogger, Exception> _jsonInputFormatterCrashed;

        private static readonly Action<ILogger, string, Exception> _jsonResultExecuting;

        static FacadeJsonLoggerExtensions()
        {
            _jsonInputFormatterCrashed = LoggerMessage.Define(
                LogLevel.Debug,
                1,
                "JSON input formatter threw an exception.");

            _jsonResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing JsonResult, writing value {Value}.");
        }

        public static void JsonInputException(this ILogger logger, Exception exception)
        {
            _jsonInputFormatterCrashed(logger, exception);
        }

        public static void JsonResultExecuting(this ILogger logger, object value)
        {
            _jsonResultExecuting(logger, Convert.ToString(value), null);
        }
    }
}