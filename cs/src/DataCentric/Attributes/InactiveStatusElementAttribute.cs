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
    /// Designates a boolean element to indicate inactive status of the
    /// record.
    ///
    /// * True value of this element corresponds to inactive status
    /// * Null (in case of nullable double) and False values of this
    ///   element correspond to active status.
    ///
    /// Depending on settings, the user interface may hide records where
    /// this element is set to true, or display them with a visual hint
    /// such as color that would indicate their inactive status.
    ///
    /// The code may also check this element to provide warning that an
    /// inactive record is being used.
    ///
    /// This attribute must be set for the root data type of a collection.
    /// Root data type is the type derived directly from TypedRecord.
    /// 
    /// Example: InactiveStatusElement["InactiveElementName"]
    ///
    /// Empty definition string is not permitted.
    ///
    /// Providing the definition for the class rather than for the
    /// element makes it possible to detect conflicting definitions of the
    /// attribute at compile time. This would have been possible if the
    /// attribute were defined at element level.
    ///
    /// The parser will check that the designated element exists, error
    /// message otherwise.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class InactiveStatusElementAttribute : Attribute
    {
        /// <summary>
        /// Name of the element designated to indicate inactive
        /// status of a record.
        /// </summary>
        public string ElementName { get; set; }

        //--- CONSTRUCTORS

        /// <summary>
        /// Create from the name of the element designated to
        /// indicate inactive status of a record.
        /// </summary>
        public InactiveStatusElementAttribute(string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
                throw new Exception("InactiveStatusElement attribute cannot be constructed from an empty string.");

            ElementName = elementName;
        }
    }
}