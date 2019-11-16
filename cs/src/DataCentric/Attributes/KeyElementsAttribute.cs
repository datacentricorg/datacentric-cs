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
using System.Reflection;

namespace DataCentric
{
    /// <summary>
    /// Create key from comma separated list of key elements in the
    /// order listed in the attribute definition, or example:
    ///
    /// This method must be set for the root data type of a collection.
    /// Root data type is the type derived directly from TypedRecord.
    ///
    /// * A is a simple primary key consisting of element A;
    /// * A, B is a complex primary key consisting of elements A, B.
    ///
    /// Empty definition string is permitted and means the record is
    /// a singleton.
    ///
    /// Providing the definition for the class rather than for the
    /// element makes it possible to:
    ///
    /// * Define a singleton record which has an empty primary key;
    /// * Define a key that includes Id field of the Record type; and
    /// * Detect conflicting definitions of the attribute at compile time
    ///
    /// None of these would have been possible if key elements were
    /// defined based on a property rather than class attribute.
    ///
    /// The parser will check that each element exists in type TRecord
    /// or its base types, error message otherwise.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class KeyElementsAttribute : Attribute
    {        
        /// <summary>
        /// Create key from comma separated list of key elements in the
        /// order listed in the attribute definition, or example:
        ///
        /// * A is a simple primary key consisting of element A;
        /// * A, B is a complex primary key consisting of elements A, B.
        ///
        /// Empty definition string means the record is a singleton.
        /// </summary>
        public string Definition { get; set; }

        //--- CONSTRUCTORS

        /// <summary>
        /// Create key from comma separated list of key elements in the
        /// order listed in the attribute definition, or example:
        ///
        /// * A is a simple primary key consisting of element A;
        /// * A, B is a complex primary key consisting of elements A, B.
        ///
        /// Empty definition string means the record is a singleton.
        /// </summary>
        public KeyElementsAttribute(string definition)
        {
            Definition = definition;
        }
    }
}