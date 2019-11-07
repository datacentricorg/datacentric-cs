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
    /// <summary>Extension methods for NodaTime.IsoDayOfWeek.</summary>
    public static class IsoDayOfWeekExtensions
    {
        /// <summary>
        /// Return true unless equal to IsoDayOfWeek.None.
        ///
        /// Define this method to override the default implementation
        /// of Enum.HasValue() that returns true for all values.
        /// </summary>
        public static bool HasValue(this IsoDayOfWeek value)
        {
            return value != IsoDayOfWeek.None;
        }

        /// <summary>Return false if null or equal to the default constructed value.</summary>
        public static bool HasValue(this IsoDayOfWeek? value)
        {
            return value.HasValue && value.HasValue();
        }

        /// <summary>Error message if equal to the default constructed value.</summary>
        public static void CheckHasValue(this IsoDayOfWeek value)
        {
            if (!value.HasValue()) throw new Exception("Required IsoDayOfWeek value is not set.");
        }

        /// <summary>Error message if null or equal to the default constructed value.</summary>
        public static void CheckHasValue(this IsoDayOfWeek? value)
        {
            if (!value.HasValue()) throw new Exception("Required IsoDayOfWeek value is not set.");
        }
    }
}
