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
    /// <summary>Static helper class for LocalMinute.</summary>
    public static class LocalMinuteUtil
    {
        /// <summary>Strict ISO 8601 time pattern with fractional seconds to millisecond precision.</summary>
        public static LocalTimePattern Pattern { get; } = LocalTimePattern.CreateWithInvariantCulture("HH':'mm");

        /// <summary>Parse string using standard ISO 8601 time pattern hh:mm:ss.fff, throw if invalid format.
        /// No variations from the standard format are accepted and no delimiters can be changed or omitted.
        /// Specifically, ISO int-like string using hhmmssfff format without delimiters is not accepted.</summary>
        public static LocalMinute Parse(string value)
        {
            // Parse local time first
            var parseResult = Pattern.Parse(value);
            var localTime = parseResult.GetValueOrThrow();

            // Then convert to local minute
            var result = localTime.ToLocalMinute();
            return result;
        }

        /// <summary>Parse ISO 8601 4 digit int in hhmm format, throw if invalid format.</summary>
        public static LocalMinute FromIsoInt(int value)
        {
            // Extract
            int hour = value / 100;
            value -= hour * 100;
            int minute = value;

            // Create new LocalMinute object, validates values on input
            var result = new LocalMinute(hour, minute);
            return result;
        }
    }
}
