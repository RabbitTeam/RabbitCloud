using System;
using System.Text.RegularExpressions;

namespace Rabbit.Cloud.Abstractions.Utilities
{
    public class TimeUtilities
    {
        public static TimeSpan GetTimeSpanBySimple(string time)
        {
            if (string.IsNullOrEmpty(time))
                throw new ArgumentException(nameof(time));

            time = time.ToLower();

            var match = Regex.Match(time, "(\\d+\\.?\\d*)([a-z]*)");
            var groups = match.Groups;

            const int numberIndex = 1;
            const int unitIndex = numberIndex + 1;

            var numberString = groups[numberIndex].Value;
            var unit = groups[unitIndex].Value;

            if (string.IsNullOrWhiteSpace(unit))
                unit = "ms";

            if (!double.TryParse(numberString, out var number))
                throw new FormatException("time format is incorrect");

            return GetTimeSpan(number, unit);
        }

        /// <summary>
        /// Get TimeSpan By <paramref name="number"/> and <paramref name="unit"/>.
        /// </summary>
        /// <param name="number">a number.</param>
        /// <param name="unit">h:hours,m:minutes,s:seconds,ms:millisecond.</param>
        /// <returns>TimeSpan.</returns>
        public static TimeSpan GetTimeSpan(double number, string unit)
        {
            if (string.IsNullOrEmpty(unit))
                throw new ArgumentNullException(nameof(unit));

            switch (unit.ToLower())
            {
                case "h":
                    return TimeSpan.FromHours(number);

                case "m":
                    return TimeSpan.FromMinutes(number);

                case "s":
                    return TimeSpan.FromSeconds(number);

                case "ms":
                    return TimeSpan.FromMilliseconds(number);
            }
            throw new FormatException("unknown units.");
        }
    }
}