/*
Copyright (C) 2013-present The DataCentric Authors.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using NodaTime;
using NodaTime.Text;

namespace DataCentric
{
    /// <summary>Static helper class for Instant.</summary>
    public static class InstantUtil
    {
        /// <summary>Default constructed Instant is treated as empty.</summary>
        public static Instant Empty { get; } = default;

        /// <summary>
        /// Strict ISO 8601 datetime pattern to millisecond precision in UTC timezone:
        ///
        /// yyyy-mm-ddThh:mm::ss.fffZ
        /// </summary>
        public static InstantPattern Pattern { get; } = InstantPattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss.FFF'Z'");

        /// <summary>
        /// Parse strict ISO 8601 datetime pattern to millisecond precision in UTC (Z) timezone:
        ///
        /// yyyy-mm-ddThh:mm::ss.fffZ
        ///
        /// Error message if the string does not match the specified format, or timezone is not Z.
        /// This class is not intended for working with timezones; use ZonedDateTime instead.
        /// 
        /// No variations from the standard format are accepted and no delimiters can be changed or omitted.
        /// Specifically, ISO int-like string in yyyymmddhhmmssfff format without delimiters is not accepted.
        /// </summary>
        public static Instant Parse(string value)
        {
            if (TryParse(value, out Instant result))
            {
                return result;
            }
            else
                throw new Exception(
                    $"Cannot parse serialized Instant {value} because it does not have " +
                    $"strict ISO 8601 datetime pattern to millisecond precision in UTC timezone: " +
                    $"yyyy-mm-ddThh:mm::ss.fffZ, or if timezone is not Z.");
        }

        /// <summary>
        /// Try parsing strict ISO 8601 datetime pattern to millisecond precision in UTC timezone:
        ///
        /// yyyy-mm-ddThh:mm::ss.fffZ
        ///
        /// * If parsing succeeds. populate the result and return true
        /// * If parsing fails, set result to InstantUtil.Empty and return false
        ///
        /// Returns false if the string does not match the specified format, or timezone is not Z.
        /// This class is not intended for working with timezones; use ZonedDateTime instead.
        /// 
        /// No variations from the standard format are accepted and no delimiters can be changed or omitted.
        /// Specifically, ISO int-like string in yyyymmddhhmmssfff format without delimiters is not accepted.
        /// </summary>
        public static bool TryParse(string value, out Instant result)
        {
            var parseResult = Pattern.Parse(value);
            if (parseResult.TryGetValue(InstantUtil.Empty, out result))
            {
                // Serialization of default constructed datetime is accepted.
                // In this case InstantUtil.Empty will be returned.
                return true;
            }
            else
            {
                result = InstantUtil.Empty;
                return false;
            }
        }

        /// <summary>
        /// Parse ISO 8601 long with millisecond precision using yyyymmddhhmmssfff format.
        ///
        /// This method assumes that value to be parsed is defined in UTC timezone.
        ///
        /// Error message if the long does not match format.
        /// </summary>
        public static Instant FromIsoLong(long value)
        {
            // Parse to LocalDateTime first, them convert to UTC instant.
            LocalDateTime localDateTime = LocalDateTimeUtil.FromIsoLong(value);
            Instant result = localDateTime.ToInstant(DateTimeZone.Utc);
            return result;
        }

        /// <summary>
        /// Creates a new instance of Instant from LocalDateTime in UTC.
        /// </summary>
        public static Instant Utc(int year, int month, int day, int hour, int minute)
        {
            return Utc(year, month, day, hour, minute, 0, 0);
        }

        /// <summary>
        /// Creates a new instance of Instant from:
        /// 
        /// Year, Month, Day, Hour, and Minute
        ///
        /// in UTC.
        /// </summary>
        public static Instant Utc(LocalDateTime localDateTime)
        {
            // Convert to Instant in UTC timezone.
            var result = localDateTime.ToInstant(DateTimeZone.Utc);
            return result;
        }

        /// <summary>
        /// Creates a new instance of Instant from:
        /// 
        /// Year, Month, Day, Hour, Minute, and Second
        ///
        /// in UTC.
        /// </summary>
        public static Instant Utc(int year, int month, int day, int hour, int minute, int second)
        {
            return Utc(year, month, day, hour, minute, second, 0);
        }

        /// <summary>
        /// Creates a new instance of Instant from:
        /// 
        /// Year, Month, Day, Hour, Minute, Second, and Millisecond
        ///
        /// in UTC.
        /// </summary>
        public static Instant Utc(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            // Create local date from the specified fields
            var localDateTime = new LocalDateTime(year, month, day, hour, minute, second, millisecond);

            // Convert to Instant in UTC timezone.
            var result = InstantUtil.Utc(localDateTime);
            return result;
        }

        /// <summary>
        /// Creates a new instance of Instant from:
        /// 
        /// Year, Month, Day, Hour, and Minute
        ///
        /// in the specified timezone.
        /// </summary>
        public static Instant Zoned(int year, int month, int day, int hour, int minute, DateTimeZone timeZone)
        {
            return Zoned(year, month, day, hour, minute, 0, 0, timeZone);
        }

        /// <summary>
        /// Create a new instance of Instant from LocalDateTime in the specified timezone.
        ///
        /// Use timeZone = DateTimeZone.Utc for the UTC timezone.
        /// </summary>
        public static Instant Zoned(LocalDateTime localDateTime, DateTimeZone timeZone)
        {
            // Convert to instant using the specified timezone
            var result = localDateTime.ToInstant(timeZone);
            return result;
        }

        /// <summary>
        /// Create a new instance of Instant from:
        /// 
        /// Year, Month, Day, Hour, Minute, and Second
        ///
        /// in the specified timezone.
        ///
        /// Use timeZone = DateTimeZone.Utc for the UTC timezone.
        /// </summary>
        public static Instant Zoned(int year, int month, int day, int hour, int minute, int second, DateTimeZone timeZone)
        {
            return Zoned(year, month, day, hour, minute, second, 0, timeZone);
        }

        /// <summary>
        /// Create a new instance of Instant from:
        /// 
        /// Year, Month, Day, Hour, Minute, Second, and Millisecond
        ///
        /// in the specified timezone.
        ///
        /// Use timeZone = DateTimeZone.Utc for the UTC timezone.
        /// </summary>
        public static Instant Zoned(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeZone timeZone)
        {
            // Create local date from the specified fields
            var localDateTime = new LocalDateTime(year, month, day, hour, minute, second, millisecond);

            // Convert to Instant in the specified timezone
            var result = InstantUtil.Zoned(localDateTime, timeZone);
            return result;
        }
    }
}
