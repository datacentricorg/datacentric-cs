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
using System.Linq.Expressions;
using NodaTime;

namespace DataCentric
{
    /// <summary>
    /// LocalMinute is an immutable struct representing a time of day
    /// to one minute precision, with no reference to a particular calendar,
    /// time zone or date.
    ///
    /// This class is not part of NodaTime but is inspired by NodaTime.LocalTime
    /// and follows the NodaTime naming conventions.
    /// </summary>
    public struct LocalMinute : IEquatable<LocalMinute>, IComparable<LocalMinute>, IComparable
    {
        //--- CONSTRUCTORS

        /// <summary>
        /// Creates local time to one minute precision from the specified hour and minute.
        /// </summary>
        public LocalMinute(int hour, int minute)
        {
            if (hour < 0 || hour > 23) throw new Exception($"Hour {hour} specified in LocalMinute constructor is not between 0 and 23.");
            if (minute < 0 || minute > 59) throw new Exception($"Minute {minute} specified in LocalMinute constructor is not between 0 and 59.");

            Hour = hour;
            Minute = minute;
        }

        //--- PROPERTIES

        /// <summary>The hour of day, in the range 0 to 23 inclusive.</summary>
        public int Hour { get; }

        /// <summary>The minute of the hour, in the range 0 to 59 inclusive.</summary>
        public int Minute { get; }

        /// <summary>The minute of the day, in the range 0 to 59 inclusive.</summary>
        public int MinuteOfDay
        {
            get { return 60 * Hour + Minute; }
        }

        //--- METHODS

        /// <summary>Converts this LocalMinute to LocalTime.</summary>
        public LocalTime ToLocalTime()
        {
            return new LocalTime(Hour, Minute);
        }

        //--- OPERATORS

        /// <summary>
        /// Compares two local times for equality, by checking whether
        /// they represent the exact same local time, down to the tick.
        /// </summary>
        public static bool operator ==(LocalMinute lhs, LocalMinute rhs)
        {
            return lhs.MinuteOfDay == rhs.MinuteOfDay;
        }

        /// <summary>Compares two local times for inequality.</summary>
        public static bool operator !=(LocalMinute lhs, LocalMinute rhs)
        {
            return lhs.MinuteOfDay != rhs.MinuteOfDay;
        }

        /// <summary>
        /// Compares two LocalMinute values to see if the left one
        /// is strictly earlier than the right one.
        /// </summary>
        public static bool operator <(LocalMinute lhs, LocalMinute rhs)
        {
            return lhs.MinuteOfDay < rhs.MinuteOfDay;
        }

        /// <summary>
        /// Compares two LocalMinute values to see if the left one
        /// is earlier than or equal to the right one.
        /// </summary>
        public static bool operator <=(LocalMinute lhs, LocalMinute rhs)
        {
            return lhs.MinuteOfDay <= rhs.MinuteOfDay;
        }

        /// <summary>
        /// Compares two LocalMinute values to see if the left one
        /// is strictly later than the right one.
        /// </summary>
        public static bool operator >(LocalMinute lhs, LocalMinute rhs)
        {
            return lhs.MinuteOfDay > rhs.MinuteOfDay;
        }

        /// <summary>
        /// Compares two LocalMinute values to see if the left one
        /// is later than or equal to the right one.
        /// </summary>
        public static bool operator >=(LocalMinute lhs, LocalMinute rhs)
        {
            return lhs.MinuteOfDay >= rhs.MinuteOfDay;
        }

        /// <summary>
        /// Indicates whether this time is earlier, later or the same as another one.
        /// </summary>
        public int CompareTo(LocalMinute other)
        {
            return this.MinuteOfDay.CompareTo(other.MinuteOfDay);
        }

        /// <summary>
        /// Indicates whether this time is earlier, later or the same as another one.
        /// </summary>
        int IComparable.CompareTo(object obj)
        {
            // Any object is greater than null
            if (obj == null) return 1;

            // Error message if comparing to a different type
            if (!(obj is LocalMinute)) throw new Exception($"Object {nameof(obj)} must be of type NodaTime.LocalMinute.");

            // Use type-specific comparison
            return this.CompareTo((LocalMinute)obj);
        }

        /// <summary>Returns a hash code for this local time.</summary>
        public override int GetHashCode()
        {
            return this.MinuteOfDay.GetHashCode();
        }

        /// <summary>
        /// Compares this local time with the specified one for equality,
        /// by checking whether the two values represent the exact same
        /// local minute.
        /// </summary>
        public bool Equals(LocalMinute other)
        {
            return this == other;
        }

        /// <summary>
        /// Compares this local time with the specified reference. A local time is
        /// only equal to another local time with the same underlying tick value.
        /// </summary>
        public override bool Equals(object obj)
        {
            // If the same type, use typed comparison
            if (obj is LocalMinute) return this == (LocalMinute)obj;

            // Different types are never equal
            return false;
        }
    }

    /// <summary>Extension methods for NodaTime.LocalMinute.</summary>
    public static class LocalMinuteExtensions
    {
        /// <summary>
        /// Unlike LocalDate and LocalDateTime, the LocalMinute class
        /// has no special value that can be treated as Empty.
        /// Its default constructed value is 00:00 (midnight).
        /// </summary>
        public static bool HasValue(this LocalMinute value)
        {
            return true;
        }

        /// <summary>Convert LocalMinute to ISO 8601 4 digit int hhmm format.</summary>
        public static int ToIsoInt(this LocalMinute value)
        {
            // Serialized to one minute precision in ISO 8601 4 digit int hhmm format
            int result = value.Hour * 100 + value.Minute;
            return result;
        }

        /// <summary>Convert LocalMinute to ISO 8601 string in hh:mm format.</summary>
        public static string ToIsoString(this LocalMinute value)
        {
            // LocalMinute is serialized to ISO 8601 string in hh:mm format
            string result = String.Join(":", value.Hour.ToString("00"), value.Minute.ToString("00"));
            return result;
        }

        /// <summary>Convert LocalMinute to variant.</summary>
        public static Variant ToVariant(this LocalMinute value)
        {
            return new Variant(value);
        }
    }
}
