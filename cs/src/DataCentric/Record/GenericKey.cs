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
using System.Linq;
using System.Reflection;
using NodaTime;

namespace DataCentric
{
    /// <summary>
    /// Generic key is used to specify the table and type-specific key
    /// together using semicolon delimited string:
    ///
    /// Table;KeyElement1;KeyElement2
    ///
    /// Generic key should be used only in those cases where the foreign key
    /// must apply to an arbitrary type. In all other cases, the regular
    /// type specific key should be used instead.
    /// </summary>
    public sealed class GenericKey
    {
        /// <summary>
        /// Create with empty Table and Key properties.
        /// </summary>
        public GenericKey() { }

        /// <summary>
        /// Populate generic key elements by parsing semicolon delimited
        /// generic key string:
        ///
        /// Table;KeyElement1;KeyElement2
        ///
        /// To avoid serialization format uncertainty, key elements
        /// can have any atomic type except Double.
        /// </summary>
        public GenericKey(string value)
        {
            AssignString(value);
        }

        //--- PROPERTIES

        /// <summary>
        /// The name of database table or collection where the
        /// record is stored.
        ///
        /// By convention, this name is the same as the name of
        /// the base record type, with module prefix and Data suffix
        /// removed. For example, if the base record class name is
        /// SettingsData, then table name is Settings.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// String key consists of semicolon delimited primary key elements:
        ///
        /// KeyElement1;KeyElement2
        ///
        /// To avoid serialization format uncertainty, key elements
        /// can have any atomic type except Double.
        /// </summary>
        public string Key { get; set; }

        //--- METHODS

        /// <summary>
        /// String representation of the generic key consists of
        /// semicolon delimited table name followed by primary key
        /// elements:
        ///
        /// Table;KeyElement1;KeyElement2
        ///
        /// To avoid serialization format uncertainty, key elements
        /// can have any atomic type except Double.
        /// </summary>
        public override string ToString() { return String.Join(";", Table, Key); }

        /// <summary>
        /// Populate generic key elements by parsing semicolon delimited
        /// generic key string:
        ///
        /// Table;KeyElement1;KeyElement2
        ///
        /// To avoid serialization format uncertainty, key elements
        /// can have any atomic type except Double.
        /// </summary>
        public void AssignString(string value)
        {
            if (!value.HasValue()) throw new Exception("Attempting to create generic key from null or empty string.");

            // Find the first delimiter, error message if not present
            // or if the delimiter is the last character in the string
            var delimiterIndex = value.IndexOf(';');
            int totalLength = value.Length;
            if (delimiterIndex == -1 || delimiterIndex == totalLength - 1)
                throw new Exception(
                    $"Generic key {value} does not use the appropriate format Table;KeyElement1;KeyElement2.");

            // The part of the string before the delimiter is table name
            Table = value.Substring(0, delimiterIndex);

            // The remaining string is type-specific key
            Key = value.Substring(delimiterIndex + 1, totalLength - delimiterIndex - 1);
        }
    }
}
