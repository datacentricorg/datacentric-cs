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

namespace DataCentric
{
    /// <summary>Extension methods for NodaTime.LocalTime.</summary>
    public static class LocalTimeExtensions
    {
        /// <summary>
        /// Unlike LocalDate and LocalDateTime, the LocalTime class
        /// has no special value that can be treated as Empty.
        /// Its default constructed value is 00:00 (midnight).
        /// </summary>
        public static bool HasValue(this LocalTime value)
        {
            return true;
        }

        /// <summary>Return false if equal to default constructed value.</summary>
        public static bool HasValue(this LocalTime? value)
        {
            return value != null;
        }

        /// <summary>Convert LocalTime to ISO 8601 9 digit int hhmmssfff format.</summary>
        public static int ToIsoInt(this LocalTime value)
        {
            // LocalTime is serialized to millisecond precision in ISO 8601 9 digit int hhmmssfff format
            int result = value.Hour * 100_00_000 + value.Minute * 100_000 + value.Second * 1000 + value.Millisecond;
            return result;
        }

        /// <summary>
        /// Use strict ISO 8601 time pattern to millisecond precision without timezone:
        ///
        /// hh:mm::ss.fff
        /// </summary>
        public static string ToIsoString(this LocalTime value)
        {
            // Use strict ISO 8601 time pattern to millisecond precision without timezone
            string result = LocalTimeUtil.Pattern.Format(value);
            return result;
        }

        /// <summary>
        /// Convert LocalTime to LocalMinute.
        ///
        /// Error message unless the local time falls exactly on the minute to nanosecond precision
        /// </summary>
        public static LocalMinute ToLocalMinute(this LocalTime value)
        {
            // Check if milliseconds is zero (that will be visible when LocalTime is serialized)
            if (value.Second != 0) throw new Exception(
                $"LocalTime {value} cannot be converted to LocalMinute because it has non-zero seconds.");
            if (value.Millisecond != 0) throw new Exception(
                $"LocalTime {value} cannot be converted to LocalMinute because it has non-zero milliseconds.");

            // Check if nanoseconds are present
            if (value.NanosecondOfSecond != 0) throw new Exception(
                $"LocalTime {value} cannot be converted to LocalMinute because it has non-zero nanoseconds of {value.NanosecondOfSecond}.");

            LocalMinute result = new LocalMinute(value.Hour, value.Minute);
            return result;
        }

        /// <summary>Convert LocalTime to variant.</summary>
        public static Variant ToVariant(this LocalTime value)
        {
            return new Variant(value);
        }
    }
}
