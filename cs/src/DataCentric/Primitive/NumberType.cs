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

namespace DataCentric
{
    /// <summary>Number type is a value type that can be serialized as a double.</summary>
    public enum NumberType
    {
        /// <summary>Empty</summary>
        Empty,

        /// <summary>Double value.</summary>
        Double,

        /// <summary>Two state boolean that can be empty or or true (serialized as 1).</summary>
        Flag,

        /// <summary>Three state boolean that can be empty, false (serialized as 0), or true (serialized as 1).</summary>
        Bool,

        /// <summary>32-bit signed integer value.</summary>
        Int,

        /// <summary>Date without time or timezone, serialized as YYYYMMDD integer.</summary>
        Date,

        /// <summary>Time without date or timezone at millisecond resolution, serialized as HHMMSSSSS integer.</summary>
        Time,

        /// <summary>Datetime without timezone at millisecond resolution, serialized as YYYYMMDDHHMMSSSSS integer.</summary>
        DateTime, 
        
        // TODO - add value for Instant

        /// <summary>Byte (char) value, serialized as a number from 0 to 255.</summary>
        Byte,

        /// <summary>Enum serialized as its 0-based index in the order defined.</summary>
        Enum
    }
}
