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
using System.Collections.Generic;

namespace DataCentric
{
    /// <summary>This comparer treats values that differ by less than Double.Tolerance as equal.</summary>
    public class DoubleComparer : IComparer<double>
    {
        /// <summary>
        /// Returns $1$ for $x \gt y$, $-1$ for $x \lt y$, and $0$ for $x==y$.
        /// Treats values that differ by less than Double.Tolerance as equal.
        /// </summary>
        public int Compare(double x, double y)
        {
            return DoubleUtil.Compare(x, y);
        }
    }
}
