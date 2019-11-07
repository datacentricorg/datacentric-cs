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
    /// <summary>Extension methods for System.Boolean.</summary>
    public static class BoolExtensions
    {
        /// <summary>Always return true as both true and false values are not empty.</summary>
        public static bool HasValue(this bool value)
        {
            return true;
        }

        /// <summary>
        /// For nullable bool,
        ///
        /// * Returns true if the value is set to true;
        /// * Returns false if null or the value is set to false.
        /// </summary>
        public static bool IsTrue(this bool? value)
        {
            return value != null && value.Value;
        }

        /// <summary>Convert bool to variant.</summary>
        public static Variant ToVariant(this bool value)
        {
            return new Variant(value);
        }
    }
}
