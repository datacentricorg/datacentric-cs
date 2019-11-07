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
    /// <summary>Variant type can hold any atomic value or be empty.</summary>
    public struct Variant : IEquatable<Variant>
    {
        private object value_;

        /// <summary>Create from object of supported types, error message if argument type is unsupported.</summary>
        public Variant(object value)
        {
            // Check that argument is one of the supported types
            switch (value)
            {
                case null:
                case string stringValue:
                case double doubleValue:
                case bool boolValue:
                case int intValue:
                case long longValue:
                    value_ = value;
                    break;
                case LocalDate dateValue:
                    if (dateValue == LocalDateUtil.Empty) throw new Exception(
                        $"Default constructed (empty) LocalDate {dateValue} has been passed to Variant(date) constructor."); // TODO - set to empty value instead of an error
                    value_ = value;
                    break;
                case LocalTime timeValue:
                    value_ = value;
                    break;
                case LocalMinute minuteValue:
                    value_ = value;
                    break;
                case LocalDateTime dateTimeValue:
                    if (dateTimeValue == LocalDateTimeUtil.Empty) throw new Exception(
                        $"Default constructed (empty) LocalDateTime {dateTimeValue} has been passed to Variant(date) constructor."); // TODO - set to empty value instead of an error
                    value_ = value;
                    break;
                case Instant instantValue:
                    if (instantValue == InstantUtil.Empty) throw new Exception(
                        $"Default constructed (empty) Instant {instantValue} has been passed to Variant(date) constructor."); // TODO - set to empty value instead of an error
                    value_ = value;
                    break;
                case Enum enumValue:
                    value_ = value;
                    break;
                default:
                    // Argument type is unsupported, error message
                    throw new Exception(GetWrongTypeErrorMessage(value));
            }
        }

        /// <summary>Type of the value held by the variant.</summary>
        public AtomicType ValueType
        {
            get
            {
                // The purpose of this check is to ensure that variant holds only one of the supported types
                switch (value_)
                {
                    case null: return AtomicType.Empty;
                    case string stringValue: return AtomicType.String;
                    case double doubleValue: return AtomicType.Double;
                    case bool boolValue: return AtomicType.Bool;
                    case int intValue: return AtomicType.Int;
                    case long longValue: return AtomicType.Long;
                    case LocalDate dateValue: return AtomicType.LocalDate;
                    case LocalTime timeValue: return AtomicType.LocalTime;
                    case LocalMinute minuteValue: return AtomicType.LocalMinute;
                    case LocalDateTime dateTimeValue: return AtomicType.LocalDateTime;
                    case Instant instantValue: return AtomicType.Instant;
                    case Enum enumValue: return AtomicType.Enum;
                    default:
                        // Error message if any other type, should normally not get to here
                        throw new Exception(GetWrongTypeErrorMessage(value_));
                }
            }
        }

        /// <summary>Check if the variant is equal to default constructed object.</summary>
        public bool IsEmpty()
        {
            return value_ == null;
        }

        /// <summary>Value held by the variant, which may be null.</summary>
        public object Value { get { return value_; } }

        /// <summary>Provides alternate serialization of certain value types.</summary>
        public override string ToString()
        {
            if (value_ != null)
            {
                // Use AsString() for custom serialization of certain value types
                return value_.AsString();
            }
            else
            {
                // Returns empty string as per standard ToString() convention, rather than null like AsString() does
                return string.Empty;
            }
        }

        /// <summary>Parse string using the specified value type and return the resulting variant.</summary>
        public static Variant Parse(AtomicType valueType, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // Empty value
                return new Variant();
            }
            else
            {
                // Switch on type of default value
                switch (valueType)
                {
                    case AtomicType.String:
                        return new Variant(value);
                    case AtomicType.Double:
                        double doubleResult = double.Parse(value);
                        return new Variant(doubleResult);
                    case AtomicType.Bool:
                        bool boolResult = bool.Parse(value);
                        return new Variant(boolResult);
                    case AtomicType.Int:
                        int intResult = int.Parse(value);
                        return new Variant(intResult);
                    case AtomicType.Long:
                        long longResult = long.Parse(value);
                        return new Variant(longResult);
                    case AtomicType.LocalDate:
                        LocalDate dateResult = LocalDateUtil.Parse(value);
                        return new Variant(dateResult);
                    case AtomicType.LocalTime:
                        LocalTime timeResult = LocalTimeUtil.Parse(value);
                        return new Variant(timeResult);
                    case AtomicType.LocalMinute:
                        LocalMinute minuteResult = LocalMinuteUtil.Parse(value);
                        return new Variant(minuteResult);
                    case AtomicType.LocalDateTime:
                        LocalDateTime dateTimeResult = LocalDateTimeUtil.Parse(value);
                        return new Variant(dateTimeResult);
                    case AtomicType.Instant:
                        Instant instantResult = InstantUtil.Parse(value);
                        return new Variant(instantResult);
                    case AtomicType.Enum:
                        throw new Exception("Variant cannot be created as enum without specifying enum typename.");
                    default:
                        // Error message if any other type
                        throw new Exception("Unknown value type when parsing string into variant.");
                }
            }
        }

        /// <summary>Parse string using the specified value type and return the resulting variant.</summary>
        public static Variant Parse<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // Empty value
                return new Variant();
            }
            else
            {
                // Switch on type of default value
                switch (default(T))
                {
                    case string stringValue:
                        return new Variant(value);
                    case double doubleValue:
                        double doubleResult = double.Parse(value);
                        return new Variant(doubleResult);
                    case bool boolValue:
                        bool boolResult = bool.Parse(value);
                        return new Variant(boolResult);
                    case int intValue:
                        int intResult = int.Parse(value);
                        return new Variant(intResult);
                    case long longValue:
                        long longResult = long.Parse(value);
                        return new Variant(longResult);
                    case LocalDate dateValue:
                        LocalDate dateResult = LocalDateUtil.Parse(value);
                        return new Variant(dateResult);
                    case LocalTime timeValue:
                        LocalTime timeResult = LocalTimeUtil.Parse(value);
                        return new Variant(timeResult);
                    case LocalMinute minuteValue:
                        LocalMinute minuteResult = LocalMinuteUtil.Parse(value);
                        return new Variant(minuteResult);
                    case LocalDateTime dateTimeValue:
                        LocalDateTime dateTimeResult = LocalDateTimeUtil.Parse(value);
                        return new Variant(dateTimeResult);
                    case Instant instantValue:
                        Instant instantResult = InstantUtil.Parse(value);
                        return new Variant(instantResult);
                    case Enum enumValue:
                        object enumResult = Enum.Parse(typeof(T), value);
                        return new Variant(enumResult);
                    default:
                        // Error message if any other type
                        throw new Exception(GetWrongTypeErrorMessage(default(T)));
                }
            }
        }

        /// <summary>Hash code is zero for null objects.</summary>
        public override int GetHashCode()
        {
            if (value_ != null) return value_.GetHashCode();
            else return 0;
        }

        /// <summary>Variants are equal when both types and values are equal.
        /// Comparison of doubles is performed with roundoff tolerance.</summary>
        public bool Equals(Variant other)
        {
            // The purpose of this check is to ensure that variant holds only one of the supported types
            switch (value_)
            {
                case null: return other.value_ == null;
                case string stringValue: return other.value_ is string && stringValue == (string) other.value_;
                case double doubleValue:
                    // Perform comparison of doubles by function that uses numerical tolerance
                    return other.value_ is double && DoubleUtil.Equal(doubleValue, (double) other.value_);
                case bool boolValue: return other.value_ is bool && boolValue == (bool) other.value_;
                case int intValue: return other.value_ is int && intValue == (int) other.value_;
                case long longValue: return other.value_ is long && longValue == (long) other.value_;
                case LocalDate dateValue: return other.value_ is LocalDate && dateValue == (LocalDate) other.value_;
                case LocalTime timeValue: return other.value_ is LocalTime && timeValue == (LocalTime) other.value_;
                case LocalMinute minuteValue: return other.value_ is LocalMinute && minuteValue == (LocalMinute) other.value_;
                case LocalDateTime dateTimeValue: return other.value_ is LocalDateTime && dateTimeValue == (LocalDateTime) other.value_;
                case Instant instantValue: return other.value_ is Instant && instantValue == (Instant)other.value_;
                case Enum enumValue:
                    // Use Equals(other) to avoid unintended reference comparison
                    return other.value_ is Enum && enumValue.Equals(other.value_);
                default:
                    // Error message if any other type, should normally not get here
                    throw new Exception(GetWrongTypeErrorMessage(value_));
            }
        }

        /// <summary>Variants are equal when both types and values are equal.
        /// Comparison of doubles is performed with roundoff tolerance.</summary>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (other.GetType() != typeof(Variant)) return false;
            return Equals((Variant)other);
        }

        /// <summary>Variants are equal when both types and values are equal.
        /// Comparison of doubles is performed with roundoff tolerance.</summary>
        public static bool operator ==(Variant lhs, Variant rhs) { return lhs.Equals(rhs); }

        /// <summary>Variants are equal when both types and values are equal.
        /// Comparison of doubles is performed with roundoff tolerance.</summary>
        public static bool operator !=(Variant lhs, Variant rhs) { return !lhs.Equals(rhs); }

        /// <summary>Provides error message about incompatible type.</summary>
        private static string GetWrongTypeErrorMessage(object value)
        {
            return string.Format(
                "Variant cannot hold {0} type. Available types are " +
                "string, double, bool, int, long, LocalDate, LocalTime, LocalMinute, LocalDateTime, Instant, or Enum.",
                value.GetType());
        }
    }
}
