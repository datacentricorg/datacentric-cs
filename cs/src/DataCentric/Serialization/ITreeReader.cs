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
    /// <summary>Interface for reading tree data.</summary>
    public interface ITreeReader
    {
        /// <summary>Read a single element (returns null if not found).
        /// Error message if more than one element with the specified name is present.</summary>
        ITreeReader ReadElement(string elementName);

        /// <summary>Read multiple elements (returns empty list if not found).</summary>
        IEnumerable<ITreeReader> ReadElements(string elementName);

        /// <summary>Read atomic value (returns empty string if not found).
        /// This overload is intended for value elements which also have
        /// attributes, otherwise ReadValueElement can be used.</summary>
        string ReadValue();

        /// <summary>Read a single element containing atomic value (returns empty string if not found).
        /// Error message if more than one element with the specified name is present.</summary>
        string ReadValueElement(string elementName);

        /// <summary>
        /// Read XML attribute (returns empty string if not found).
        ///
        /// When deserializing from a representation format that does not support
        /// attributes, such as JSON, the attribute is read as element with
        /// underscore prefix before its name.
        /// </summary>
        string ReadAttribute(string attributeName);
    }
}
