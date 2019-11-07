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
    /// <summary>Static helper class for Double.</summary>
    public static class DoubleUtil
    {
        /// <summary>Constant representing the empty value in non-nullable variables.</summary>
        public static double Empty { get { return -1.0e100; } }

        /// <summary>Constant representing double comparison tolerance</summary>
        public static double Tolerance { get { return 1.0e-10; } }

        /// <summary>Function Equal(double) using tolerance-based comparison.
        /// Treats values that differ by less than Double.Tolerance as equal.</summary>
        public static bool Equal(double lhs, double rhs) { return lhs >= rhs - Tolerance && lhs <= rhs + Tolerance; }

        /// <summary>Return $lhs > rhs$ using tolerance-based comparison.
        /// Treats values that differ by less than Double.Tolerance as equal.</summary>
        public static bool More(double lhs, double rhs) { return lhs > rhs + Tolerance; }

        /// <summary>Return $lhs >= rhs$ using tolerance-based comparison.
        /// Treats values that differ by less than Double.Tolerance as equal.</summary>
        public static bool MoreOrEqual(double lhs, double rhs) { return lhs >= rhs - Tolerance; }

        /// <summary>Return $lhs \lt rhs$ using tolerance-based comparison.
        /// Treats values that differ by less than Double.Tolerance as equal.</summary>
        public static bool Less(double lhs, double rhs) { return lhs < rhs - Tolerance; }

        /// <summary>
        /// Return $lhs \le rhs$ using tolerance-based comparison.
        /// Treats values that differ by less than Double.Tolerance as equal.
        /// </summary>
        public static bool LessOrEqual(double lhs, double rhs) { return lhs <= rhs + Tolerance; }

        /// <summary>Returns $1$ for $x \gt y$, $-1$ for $x \lt y$, and $0$ for $x==y$.
        /// Treats values that differ by less than Double.Tolerance as equal.</summary>
        public static int Compare(double x, double y)
        {
            if (x > y + DoubleUtil.Tolerance) return 1;
            else if (x < y - DoubleUtil.Tolerance) return -1;
            else return 0;
        }
    }
}
