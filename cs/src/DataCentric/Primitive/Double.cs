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
using System.Globalization;

namespace DataCentric
{
    /// <summary>Extension methods for System.Double.</summary>
    public static class DoubleExtensions
    {
        /// <summary>Return true unless equal to Double.Empty.</summary>
        public static bool HasValue(this double value)
        {
            return value != DoubleUtil.Empty;
        }

        /// <summary>Return true if the value is within roundoff tolerance from a 32 bit integer.</summary>
        public static bool IsInt(this double obj)
        {
            double rounded = Math.Round(obj);
            if (rounded > Int32.MaxValue || rounded < Int32.MinValue) throw new Exception($"Double value {obj} is too large for conversion to 32 bit integer");
            bool result = rounded <= obj + DoubleUtil.Tolerance && rounded >= obj - DoubleUtil.Tolerance;
            return result;
        }

        /// <summary>
        /// Convert double to int if it is within roundoff tolerance
        /// from a 32 bit integer, otherwise error message.
        /// </summary>
        public static int ToInt(this double obj)
        {
            double rounded = Math.Round(obj);
            if (rounded > Int32.MaxValue || rounded < Int32.MinValue)
                throw new Exception($"Double value {obj} is too large for conversion to 32 bit integer");
            if (rounded > obj + DoubleUtil.Tolerance || rounded < obj - DoubleUtil.Tolerance)
                throw new Exception($"Double value {obj} is more than roundoff tolerance away from a 32 bit integer");

            return (int) rounded;
        }

        /// <summary>
        /// Convert nullable double only if within roundoff tolerance
        /// from a 64 bit integer, otherwise error message.
        /// </summary>
        public static long ToLong(this double obj)
        {
            double rounded = Math.Round(obj);
            if (rounded > Int64.MaxValue || rounded < Int64.MinValue)
                throw new Exception($"Double value {obj} is too large for conversion to 64 bit integer");
            if (rounded > obj + DoubleUtil.Tolerance || rounded < obj - DoubleUtil.Tolerance)
                throw new Exception($"Double value {obj} is more than roundoff tolerance away from a 64 bit integer");

            return (long) rounded;
        }

        /// <summary>Format for display purposes only, not suitable for serialization roundtrip.</summary>
        public static string DisplayFormat(this double value) // TODO Rename and potentially implement as object extension method?
        {
            // For $abs(x)<1$, use the maximum of 6 decimal points;
            // for $1\le abs(x)<100$, 4 decimal points;
            // for $abs(x)\ge 100$, 2 decimal points
            double absValue = Math.Abs(value);
            var cultureInfo = CultureInfo.InvariantCulture;
            string result = null;
            if (DoubleUtil.Less(absValue, 1.0)) result = String.Format(cultureInfo, "{0:0.######}", value);
            else if (DoubleUtil.Less(absValue, 100.0)) result = String.Format(cultureInfo, "{0:0.####}", value);
            else result = String.Format(cultureInfo, "{0:0.##}", value);

            return result;
        }
    }
}
