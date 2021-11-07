using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsNullOrEmpty(this DateTime? self)
        {
            return !self.HasValue || self == DateTime.MinValue;
        }

        public static bool IsEmpty(this DateTime self)
        {
            return self == DateTime.MinValue;
        }

        public static DateTime Time(this DateTime date, DateTime time)
        {
            return date.Time(time.Hour, time.Minute, time.Second, time.Millisecond);
        }

        public static DateTime Time(this DateTime date, int hour, int minute = 0, int second = 0, int millisecond = 0)
        {
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, second, millisecond);
        }

        public static DateTime NullifyDate(this DateTime date, bool sql = false)
        {
            var nullDate = !sql ? DateTime.MinValue : (DateTime)SqlDateTime.MinValue;
            return nullDate.Time(date.Hour, date.Minute, date.Second);
        }

        public static DateTime NullifyTime(this DateTime date)
        {
            return date.Time(0);
        }

        public static long GetJsTime(this DateTime self)
        {
            return (long)((self.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds + 0.5);
        }

        public static DateTime GetDateWithEndOfDayTime(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, date.Kind);
        }

        public static bool IsAm(this DateTime self)
        {
            return self.ToString("tt").EqualsIgnoringCase("AM");
        }

        public static string ToTimeString(this TimeSpan duration)
        {
            if (duration == TimeSpan.Zero)
                return "N/A";

            if (duration.Days == 1 || (duration.Days == 0 && duration.Minutes == 0))
                return $"{duration.TotalHours:0.#} h.";

            if (duration.Days == 0 && duration.Hours != 0 && duration.Minutes != 0)
                return $"{duration.Hours} h. {duration.Minutes} min.";

            if (duration.Hours == 0)
                return $"{duration.Minutes} min.";

            return $"{duration.TotalDays:0.#} days";
        }

        public static string ToShortTimeOfDayString(this TimeSpan time)
        {
            return DateTime.Today.Add(time).ToString("h:mm tt");
        }

        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
                throw new ArgumentException($"{nameof(timeSpan)} cannot be Timespan.Zero");

            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        public static DateTime FromUtc(this DateTime self, string toTimeZoneId)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(self, TimeZoneInfo.FindSystemTimeZoneById(toTimeZoneId));
        }

        public static DateTime ToUtc(this DateTime self, string fromTimeZoneId)
        {
            return TimeZoneInfo.ConvertTimeToUtc(self, TimeZoneInfo.FindSystemTimeZoneById(fromTimeZoneId));
        }

        public static string ToShortDateTimeString(this DateTime self)
        {
            return $"{self.ToShortDateString()} {self.ToShortTimeString()}";
        }
    }
}
