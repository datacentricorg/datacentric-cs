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
    /// <summary>Extension methods for System.Object.</summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Return true is the object is:
        ///
        /// * Null; or
        /// * For certain atomic value types, equal to a known
        ///   special value treated as empty
        ///
        /// Return false in all other cases, including for all
        /// non-null references to classes, structs, or enums.
        ///
        /// This method cannot detect string patterns inside
        /// Enum such as None or Empty and will treat these values
        /// as not empty. Use nullable enums to store optional
        /// enum values.
        /// </summary>
        public static bool IsEmpty(this object obj)
        {
            switch(obj)
            {
                case null:
                    // Null never has value
                    return true;
                case string stringValue:
                    return !stringValue.HasValue();
                case double doubleValue:
                    return !doubleValue.HasValue();
                case bool boolValue:
                    return !boolValue.HasValue();
                case int intValue:
                    return !intValue.HasValue();
                case long longValue:
                    return !longValue.HasValue();
                case LocalDate dateValue:
                    return !dateValue.HasValue();
                case LocalTime timeValue:
                    return !timeValue.HasValue();
                case LocalMinute minuteValue:
                    return !minuteValue.HasValue();
                case LocalDateTime dateTimeValue:
                    return !dateTimeValue.HasValue();
                case Instant instantValue:
                    return !instantValue.HasValue();
                case TemporalId temporalIdValue:
                    return !temporalIdValue.HasValue();
                default:
                    // If not null and does not have a known HasValue method,
                    // treat as object that is not empty
                    return false;
            }
        }

        /// <summary>
        /// Use in place of ToString() for alternate serialization of certain types.
        ///
        /// This method returns String.Empty when the argument is null or equal to
        /// a special value treated as empty.
        /// </summary>
        public static string AsString(this object obj)
        {
            switch (obj)
            {
                case null:
                    // Return String.Empty when the argument is null
                    return String.Empty;
                case string stringValue:
                    // Return the value itself
                    return stringValue;
                case double doubleValue:
                    // Standard formatting for double
                    return doubleValue.ToString();
                case bool boolValue:
                    // Uses lowercase true and false as per JSON convention, rather
                    // than True and False return by the standard C# ToString()
                    if (boolValue) return "true";
                    else return "false";
                case int intValue:
                    // Standard formatting for int
                    return intValue.ToString();
                case long longValue:
                    // Standard formatting for long
                    return longValue.ToString();
                case LocalDate dateValue:
                    // Return ISO 8601 string using yyyy-mm-dd format
                    return dateValue.ToIsoString();
                case LocalTime timeValue:
                    // Return ISO 8601 string using hh:mm:ss.fff format
                    return timeValue.ToIsoString();
                case LocalMinute minuteValue:
                    // Return ISO 8601 string using hh:mm format
                    return minuteValue.ToIsoString();
                case LocalDateTime dateTimeValue:
                    // Return to ISO 8601 string using yyyy-mm-ddThh:mm::ss.fff format
                    return dateTimeValue.ToIsoString();
                case Instant instantValue:
                    // Return to ISO 8601 string using yyyy-mm-ddThh:mm::ss.fffZ format in UTC
                    return instantValue.ToIsoString();
                case IsoDayOfWeek isoDayOfWeekValue:
                    // Use short three-letter format for the day of week
                    switch (isoDayOfWeekValue)
                    {
                        // The value of IsoDayOfWeek.None is converted to empty string
                        case IsoDayOfWeek.None: return String.Empty;
                        case IsoDayOfWeek.Monday: return "Mon";
                        case IsoDayOfWeek.Tuesday: return "Tue";
                        case IsoDayOfWeek.Wednesday: return "Wed";
                        case IsoDayOfWeek.Thursday: return "Thu";
                        case IsoDayOfWeek.Friday: return "Fri";
                        case IsoDayOfWeek.Saturday: return "Sat";
                        case IsoDayOfWeek.Sunday: return "Sun";
                        default:
                            throw new Exception($"Unknown value {isoDayOfWeekValue} for NodaTime.IsoDayOfWeek enum.");
                    }
                default:
                    // In all other cases, return ToString()
                    return obj.ToString();
            }
        }

        /// <summary>
        /// Syntactic sugar for ``obj is T'' that is more convenient
        /// to use in functional programming than the native syntax.
        /// </summary>
        public static bool Is<T>(this object obj)
            where T : class
        {
            return obj is T;
        }

        /// <summary>
        /// Syntactic sugar for ``obj as T'' that is more convenient
        /// to use in functional programming than the native syntax.
        ///
        /// Returns null if the conversion fails.
        /// </summary>
        public static T As<T>(this object obj)
            where T : class
        {
            return obj as T;
        }

        /// <summary>
        /// Syntactic sugar for ``(T)obj'' that is more convenient
        /// to use in functional programming than the native syntax.
        ///
        /// Error message if the cast fails.
        /// </summary>
        public static T CastTo<T>(this object obj)
            where T : class
        {
            if (obj is T result)
            {
                return result;
            }
            else
            {
                throw new Exception($"Cannot convert {obj.GetType().Name} to {typeof(T).Name}.");
            }
        }
    }
}
