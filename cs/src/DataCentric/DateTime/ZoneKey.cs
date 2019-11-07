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
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace DataCentric
{
    /// <summary>
    /// This class provides timezone conversion between UTC
    /// and local datetime for the specified timezone.
    ///
    /// Only the following timezone names are permitted:
    ///
    /// * UTC; and
    /// * IANA city timezones such as America/New_York
    ///
    /// Other 3-letter regional timezones such as EST or EDT are
    /// not permitted because they do not handle the transition
    /// between winter and summer time automatically for those
    /// regions where winter time is defined.
    ///
    /// Because ZoneName is used to look up timezone conventions,
    /// it must match either the string UTC or the code in IANA
    /// timezone database precisely. The IANA city timezone code
    /// has two slash-delimited tokens, the first referencing the
    /// country and the other the city, for example America/New_York.
    /// </summary>
    [BsonSerializer(typeof(BsonKeySerializer<ZoneKey>))]
    public sealed class ZoneKey : TypedKey<ZoneKey, Zone>
    {
        /// <summary>
        /// Unique timezone name.
        ///
        /// Only the following timezone names are permitted:
        ///
        /// * UTC; and
        /// * IANA city timezones such as America/New_York
        ///
        /// Other 3-letter regional timezones such as EST or EDT are
        /// not permitted because they do not handle the transition
        /// between winter and summer time automatically for those
        /// regions where winter time is defined.
        ///
        /// Because ZoneName is used to look up timezone conventions,
        /// it must match either the string UTC or the code in IANA
        /// timezone database precisely. The IANA city timezone code
        /// has two slash-delimited tokens, the first referencing the
        /// country and the other the city, for example America/New_York.
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// Returns NodaTime timezone object used for conversion between
        /// UTC and local date, time, minute, and datetime.
        ///
        /// Only the following timezone names are permitted:
        ///
        /// * UTC; and
        /// * IANA city timezones such as America/New_York
        ///
        /// Other 3-letter regional timezones such as EST or EDT are
        /// not permitted because they do not handle the transition
        /// between winter and summer time automatically for those
        /// regions where winter time is defined.
        ///
        /// Because ZoneName is used to look up timezone conventions,
        /// it must match either the string UTC or the code in IANA
        /// timezone database precisely. The IANA city timezone code
        /// has two slash-delimited tokens, the first referencing the
        /// country and the other the city, for example America/New_York.
        /// </summary>
        public DateTimeZone GetDateTimeZone()
        {
            // Check that ZoneName is set
            if (!ZoneName.HasValue()) throw new Exception("ZoneName is not set.");

            if (ZoneName != "UTC" && !ZoneName.Contains("/"))
                throw new Exception(
                    $"ZoneName={ZoneName} is not UTC and is not a forward slash  " +
                    $"delimited city timezone. Only (a) UTC timezone and (b) IANA TZDB " +
                    $"city timezones such as America/New_York are permitted " +
                    $"as ZoneName values, but not three-symbol timezones without " +
                    $"delimiter such as EST or EDT that do not handle the switch " +
                    $"between winter and summer time automatically when winter time " +
                    $"is defined.");

            // Initialize DateTimeZone
            var result = DateTimeZoneProviders.Tzdb.GetZoneOrNull(ZoneName);

            // If still null after initialization, ZoneName was not
            // found in the IANA database of city timezone codes
            if (result == null)
                throw new Exception($"ZoneName={ZoneName} not found in IANA TZDB timezone database.");

            return result;
        }
    }
}
