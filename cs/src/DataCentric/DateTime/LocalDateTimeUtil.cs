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
    /// <summary>Static helper class for LocalDateTime.</summary>
    public static class LocalDateTimeUtil
    {
        /// <summary>Default constructed LocalDateTime is treated as empty.</summary>
        public static LocalDateTime Empty { get; } = default;

        /// <summary>
        /// Strict ISO 8601 datetime pattern to millisecond precision without timezone:
        ///
        /// yyyy-mm-ddThh:mm::ss.fff
        /// </summary>
        public static LocalDateTimePattern Pattern { get; } = LocalDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss.FFF");

        /// <summary>
        /// Parse strict ISO 8601 datetime pattern to millisecond precision without timezone:
        ///
        /// yyyy-mm-ddThh:mm::ss.fff
        ///
        /// Error message if the string does not match format.
        /// 
        /// No variations from the standard format are accepted and no delimiters can be changed or omitted.
        /// Specifically, ISO int-like string in yyyymmddhhmmssfff format without delimiters is not accepted.
        /// </summary>
        public static LocalDateTime Parse(string value)
        {
            if (TryParse(value, out LocalDateTime result))
            {
                return result;
            }
            else
                throw new Exception(
                    $"Cannot parse serialized LocalDateTime {value} because it does not have " +
                    $"strict ISO 8601 datetime pattern to millisecond precision without timezone: " +
                    $"yyyy-mm-ddThh:mm::ss.fff");
        }

        /// <summary>
        /// Try parsing strict ISO 8601 datetime pattern to millisecond precision without timezone:
        ///
        /// yyyy-mm-ddThh:mm::ss.fff
        ///
        /// * If parsing succeeds. populate the result and return true
        /// * If parsing fails, set result to LocalDateTimeUtil.Empty and return false
        /// 
        /// No variations from the standard format are accepted and no delimiters can be changed or omitted.
        /// Specifically, ISO int-like string in yyyymmddhhmmssfff format without delimiters is not accepted.
        /// </summary>
        public static bool TryParse(string value, out LocalDateTime result)
        {
            var parseResult = Pattern.Parse(value);
            if (parseResult.TryGetValue(LocalDateTimeUtil.Empty, out result))
            {
                // Serialization of default constructed datetime is accepted.
                // In this case LocalDateTimeUtil.Empty will be returned.
                return true;
            }
            else
            {
                result = LocalDateTimeUtil.Empty;
                return false;
            }
        }

        /// <summary>
        /// Parse ISO 8601 long with millisecond precision using yyyymmddhhmmssfff format.
        ///
        /// Error message if the long does not match format.
        /// </summary>
        public static LocalDateTime FromIsoLong(long value)
        {
            // Split into date and time using int64 arithmetic
            long isoDateLong = value/100_00_00_000;
            long isoTimeLong = value - 100_00_00_000 * isoDateLong;

            // Check that it will fit into Int32 range
            if (isoDateLong < Int32.MinValue || isoDateLong > Int32.MaxValue)
                throw new Exception($"Date portion of datetime {value} has invalid format.");
            if (isoTimeLong < Int32.MinValue || isoTimeLong > Int32.MaxValue)
                throw new Exception($"Time portion of datetime {value} has invalid format.");

            // Convert to Int32
            int isoDate = (int) isoDateLong;
            int isoTime = (int) isoTimeLong;

            // Extract year, month, day
            int year = isoDate / 100_00;
            isoDate -= year * 100_00;
            int month = isoDate / 100;
            isoDate -= month * 100;
            int day = isoDate;

            // Extract year, month, day
            int hour = isoTime / 100_00_000;
            isoTime -= hour * 100_00_000;
            int minute = isoTime / 100_000;
            isoTime -= minute * 100_000;
            int second = isoTime / 1000;
            isoTime -= second * 1000;
            int millisecond = isoTime;

            // Create LocalDateTime object
            var result = new LocalDateTime(year, month, day, hour, minute, second, millisecond);
            return result;
        }
    }
}
