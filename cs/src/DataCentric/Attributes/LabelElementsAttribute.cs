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
    /// Create label from a comma separated list of element(s) that will
    /// be displayed by the user interface where key elements(s) would be
    /// displayed otherwise.
    ///
    /// This method must be set for the root data type of a collection.
    /// Root data type is the type derived directly from TypedRecord.
    /// 
    /// Examples:
    ///
    /// * A means simple label A
    /// * A, B means semicolon-delimited label A;B
    ///
    /// Empty definition string is not permitted.
    ///
    /// Providing the definition for the class rather than for the
    /// element makes it possible to:
    ///
    /// * Define a label that includes Id field of the Record type; and
    /// * Detect conflicting definitions of the attribute at compile time
    ///
    /// None of these would have been possible if label elements were
    /// defined based on a property rather than class attribute.
    ///
    /// The parser will check that each element exists in type TRecord
    /// or its base types, error message otherwise.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class LabelElementsAttribute : Attribute
    {
        /// <summary>
        /// Create label from a comma separated list of element(s) that will
        /// be displayed by the user interface where key elements(s) would be
        /// displayed otherwise.
        ///
        /// Examples:
        ///
        /// * A means simple label A
        /// * A, B means semicolon-delimited label A;B
        ///
        /// Empty definition string is not permitted.
        /// </summary>
        public string Definition { get; set; }

        //--- CONSTRUCTORS

        /// <summary>
        /// Create label from a comma separated list of element(s) that will
        /// be displayed by the user interface where key elements(s) would be
        /// displayed otherwise.
        ///
        /// Examples:
        ///
        /// * A means simple label A
        /// * A, B means semicolon-delimited label A;B
        ///
        /// Empty definition string is not permitted.
        /// </summary>
        public LabelElementsAttribute(string definition)
        {
            if (string.IsNullOrEmpty(definition))
                throw new Exception("LabelElements attribute cannot be constructed from an empty string.");

            Definition = definition;
        }
    }
}