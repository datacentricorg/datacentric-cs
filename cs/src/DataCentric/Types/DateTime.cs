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
    /// <summary>Extension methods for System.DateTime.</summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Convert System.DateTime with Kind=Utc to Instant.
        ///
        /// Converts default constructed DateTime to Instant.Empty.
        ///
        /// Error message if the timezone is not UTC.
        /// </summary>
        public static Instant ToInstant(this DateTime value)
        {
            // Error message if the timezone is not UTC.
            if (value.Kind != DateTimeKind.Utc) throw new Exception("DateTime can only be converted to Instant when its Kind=UTC.");

            if (value != default)
            {
                // If not default constructed value, convert
                // to millisecond precision using fields
                return InstantUtil.Utc(
                    value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond);
            }
            else
            {
                // Converts default constructed DateTime
                // to Instant.Empty
                return InstantUtil.Empty;
            }
        }
    }
}

