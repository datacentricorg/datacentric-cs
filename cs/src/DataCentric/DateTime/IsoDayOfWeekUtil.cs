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
using System.Runtime.InteropServices.ComTypes;
using NodaTime;

namespace DataCentric
{
    /// <summary>Static helper class for NodaTime.IsoDayOfWeek.</summary>
    public static class IsoDayOfWeekUtil
    {
        /// <summary>
        /// Converts the short three-letter string representation
        /// of the day of week to NodaTime.IsoDayOfWeek enum value.
        ///
        /// This parser accepts only short three-letter abbreviations
        /// (e.g. Mon), not the full name (e.g. Monday).
        ///
        /// Set result to default enum value (None) and return
        /// false if the conversion fails.
        /// </summary>
        public static bool TryParse(string s, out IsoDayOfWeek result)
        {
            switch (s)
            {
                // Empty string is converted to IsoDayOfWeek.None
                case "":
                    result = IsoDayOfWeek.None;
                    return true;
                case "Mon":
                    result = IsoDayOfWeek.Monday;
                    return true;
                case "Tue":
                    result = IsoDayOfWeek.Tuesday;
                    return true;
                case "Wed":
                    result = IsoDayOfWeek.Wednesday;
                    return true;
                case "Thu":
                    result = IsoDayOfWeek.Thursday;
                    return true;
                case "Fri":
                    result = IsoDayOfWeek.Friday;
                    return true;
                case "Sat":
                    result = IsoDayOfWeek.Saturday;
                    return true;
                case "Sun":
                    result = IsoDayOfWeek.Sunday;
                    return true;
                default:
                    // Conversion failed, return false and set the
                    // result to the default value of None
                    result = IsoDayOfWeek.None;
                    return false;
            }
        }

        /// <summary>
        /// Converts the short three-letter string representation
        /// of the day of week to NodaTime.IsoDayOfWeek enum value.
        ///
        /// This parser accepts only short three-letter abbreviations
        /// (e.g. Mon), not the full name (e.g. Monday).
        ///
        /// Error message if the conversion fails.
        /// </summary>
        public static IsoDayOfWeek Parse(string s)
        {
            if (!TryParse(s, out IsoDayOfWeek result))
                throw new Exception($"String {s} cannot be converted to IsoDayOfWeek. This parser " +
                                    $"accepts only short three-letter abbreviations (e.g. Mon), not" +
                                    $"the full name (e.g. Monday).");
            return result;
        }
    }
}
