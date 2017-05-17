using System;

namespace RabbitCloud.Abstractions.Utilities
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime StartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public static long GetMillisecondsTimeStamp(this DateTime dateTime)
        {
            var timeSpan = dateTime.ToUniversalTime() - StartDateTime;
            return Convert.ToInt64(timeSpan.TotalMilliseconds);
        }

        public static long GetSecondsTimeStamp(this DateTime dateTime)
        {
            var timeSpan = dateTime.ToUniversalTime() - StartDateTime;
            return Convert.ToInt64(timeSpan.TotalSeconds);
        }
    }
}