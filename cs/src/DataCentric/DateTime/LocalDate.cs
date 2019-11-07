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
    /// <summary>Extension methods for NodaTime.LocalDate.</summary>
    public static class LocalDateExtensions
    {
        /// <summary>
        /// Return true unless equal to the default constructed value.
        ///
        /// Default constructed value is not a valid value for this type.
        /// </summary>
        public static bool HasValue(this LocalDate value)
        {
            return value != default;
        }

        /// <summary>Convert LocalDate to ISO 8601 int using yyyymmdd format.</summary>
        public static int ToIsoInt(this LocalDate value)
        {
            // If default constructed date is passed, error message
            if (value == LocalDateUtil.Empty) throw new Exception(
                $"Default constructed (empty) LocalDate {value} has been passed to ToIsoInt() method.");

            int result = value.Year * 10_000 + value.Month * 100 + value.Day;
            return result;
        }

        /// <summary>
        /// Use strict ISO 8601 date format:
        ///
        /// yyyy-mm-dd
        ///
        /// Return String.Empty for the default constructed value.
        /// </summary>
        public static string ToIsoString(this LocalDate value)
        {
            // If default constructed datetime is passed, error message
            if (value != LocalDateUtil.Empty)
            {
                // Use strict ISO 8601 datetime pattern to millisecond precision without timezone
                string result = LocalDateUtil.Pattern.Format(value);
                return result;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>Convert LocalDate to variant.</summary>
        public static Variant ToVariant(this LocalDate value)
        {
            return new Variant(value);
        }
    }
}
