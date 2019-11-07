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
    /// <summary>Static helper class for LocalDate.</summary>
    public static class LocalDateUtil
    {
        /// <summary>Default constructed LocalDate is treated as empty.</summary>
        public static LocalDate Empty { get; } = default;

        /// <summary>Strict ISO 8601 date pattern yyyy-mm-dd.</summary>
        public static LocalDatePattern Pattern { get; } = LocalDatePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd");

        /// <summary>
        /// Parse strict ISO 8601 date format:
        ///
        /// yyyy-mm-dd
        ///
        /// Error message if the string does not match format.
        /// 
        /// No variations from the standard format are accepted and no delimiters can be changed or omitted.
        /// Specifically, ISO int-like string in yyyymmdd format without delimiters is not accepted.
        /// </summary>
        public static LocalDate Parse(string value)
        {
            if (TryParse(value, out LocalDate result))
            {
                return result;
            }
            else
                throw new Exception(
                    $"Cannot parse serialized LocalDate {value} because it does not have " +
                    $"strict ISO 8601 date pattern: yyyy-mm-dd");
        }

        /// <summary>
        /// Try parsing strict ISO 8601 date format:
        ///
        /// yyyy-mm-dd
        ///
        /// * If parsing succeeds. populate the result and return true
        /// * If parsing fails, set result to LocalDateUtil.Empty and return false
        /// 
        /// No variations from the standard format are accepted and no delimiters can be changed or omitted.
        /// Specifically, ISO int-like string in yyyymmdd format without delimiters is not accepted.
        /// </summary>
        public static bool TryParse(string value, out LocalDate result)
        {
            var parseResult = Pattern.Parse(value);
            if (parseResult.TryGetValue(LocalDateUtil.Empty, out result))
            {
                // Serialization of default constructed date is accepted.
                // In this case LocalDateUtil.Empty will be returned.
                return true;
            }
            else
            {
                result = LocalDateUtil.Empty;
                return false;
            }
        }

        /// <summary>
        /// Parse ISO 8601 int using yyyymmdd format.
        ///
        /// 
        /// Error message if the int does not match format.
        /// </summary>
        public static LocalDate FromIsoInt(int value)
        {
            // Extract year, month, day
            int year = value / 100_00;
            value -= year * 100_00;
            int month = value / 100;
            value -= month * 100;
            int day = value;

            // Create LocalDate object
            var result = new LocalDate(year, month, day);
            return result;
        }
    }
}
