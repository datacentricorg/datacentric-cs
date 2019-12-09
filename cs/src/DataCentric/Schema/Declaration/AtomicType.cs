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

namespace DataCentric
{
    /// <summary>List of values and immutable types, including string and binary.</summary>
    public enum AtomicType
    {
        /// <summary>
        /// Indicates that enum value is not set.
        /// 
        /// In programming languages where enum defaults to the first item when
        /// not set, making Empty the first item prevents unintended assignment
        /// of a meaningful value.
        /// </summary>
        Empty,

        /// <summary>Bool value.</summary>
        Bool,

        /// <summary>Nullable bool value.</summary>
        NullableBool,

        /// <summary>Int value.</summary>
        Int,

        /// <summary>Nullable int value.</summary>
        NullableInt,

        /// <summary>Long value.</summary>
        Long,

        /// <summary>Nullable long value.</summary>
        NullableLong,

        /// <summary>Double value.</summary>
        Double,

        /// <summary>Nullable double value.</summary>
        NullableDouble,

        /// <summary>Date value.</summary>
        Date,

        /// <summary>Nullable date value.</summary>
        NullableDate,

        /// <summary>DateTime value.</summary>
        DateTime,

        /// <summary>Nullable DateTime value.</summary>
        NullableDateTime,

        /// <summary>String value.</summary>
        String,

        /// <summary>Binary value.</summary>
        Binary,

        /// <summary>Key value.</summary>
        Key,

        /// <summary>Generic data value.</summary>
        Data,

        /// <summary>Variant value.</summary>
        Variant,

        /// <summary>Decimal value.</summary>
        Decimal,

        /// <summary>Nullable decimal value.</summary>
        NullableDecimal,

        /// <summary>Time value.</summary>
        Time,

        /// <summary>Nullable time value.</summary>
        NullableTime,

        /// <summary>TemporalId.</summary>
        TemporalId,

        /// <summary>Nullable TemporalId.</summary>
        NullableTemporalId,

        /// <summary>Minute.</summary>
        Minute,

        /// <summary>Nullable minute.</summary>
        NullableMinute,

        /// <summary>Instant.</summary>
        Instant,

        /// <summary>Nullable instant.</summary>
        NullableInstant,
    }
}