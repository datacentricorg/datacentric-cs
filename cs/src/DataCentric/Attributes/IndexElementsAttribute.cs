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
    /// Use IndexElements attribute to specify database indexes
    /// for the class. A class may have more than one IndexElements
    /// attribute, each for a separate index.
    ///
    /// This method must be set for the root data type of a collection.
    /// Root data type is the type derived directly from TypedRecord.
    ///
    /// The definition string for the index is a comma separated
    /// list of element names. The elements sorted in descending
    /// order are prefixed by -.
    ///
    /// Examples:
    ///
    /// * A is an index on element A in ascending order;
    /// * -A is an index on element A in descending order;
    /// * A,B,-C is an index on elements A and B in ascending
    ///   order and then element C in descending order.
    ///
    /// When collection interface is obtained from a data source,
    /// names of the elements in the index definition are validated
    /// to match element names of the class for which the index is
    /// defined. If the class does not have an an element with the
    /// name specified as part of the definition string, an error
    /// message is given.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class IndexElementsAttribute : Attribute
    {
        /// <summary>
        /// The definition string for the index is a comma separated
        /// list of element names. The elements sorted in descending
        /// order are prefixed by -.
        ///
        /// Examples:
        ///
        /// * A is an index on element A in ascending order;
        /// * -A is an index on element A in descending order;
        /// * A,B,-C is an index on elements A and B in ascending
        ///   order and then element C in descending order.
        /// </summary>
        public string Definition { get; set; }

        /// <summary>
        /// Custom short name of the index (optional).
        ///
        /// By default, the delimited elements string (index
        /// definition) is used as index name. When the default
        /// name exceeds the maximum index name length, use
        /// this optional property to specify a shorter custom
        /// index name.
        /// </summary>
        public string Name { get; set; }

        //--- CONSTRUCTORS

        /// <summary>
        /// Create from the index definition string.
        ///
        /// The definition string for the index is a comma separated
        /// list of element names. The elements sorted in descending
        /// order are prefixed by -.
        ///
        /// Examples:
        ///
        /// * A is an index on element A in ascending order;
        /// * -A is an index on element A in descending order;
        /// * A,B,-C is an index on elements A and B in ascending
        ///   order and then element C in descending order.
        /// </summary>
        public IndexElementsAttribute(string definition)
        {
            Definition = definition;
        }

        /// <summary>
        /// Create from the index definition string, and optional
        /// custom name for the index.
        ///
        /// The definition string for the index is a comma separated
        /// list of element names. The elements sorted in descending
        /// order are prefixed by -.
        ///
        /// Examples:
        ///
        /// * A is an index on element A in ascending order;
        /// * -A is an index on element A in descending order;
        /// * A,B,-C is an index on elements A and B in ascending
        ///   order and then element C in descending order.
        ///
        /// By default, the delimited elements string (index
        /// definition) is used as index name. When the default
        /// name exceeds the maximum index name length, use
        /// this optional property to specify a shorter custom
        /// index name.
        /// </summary>
        public IndexElementsAttribute(string definition, string name)
        {
            Definition = definition;
            Name = name;
        }

        /// <summary>
        /// Get IndexedElements attributes for the class and its
        /// parents as SortedDictionary(IndexDefinition, IndexName).
        /// </summary>
        public static SortedDictionary<string, string> GetAttributesDict<TRecord>()
            where TRecord : Record
        {
            // The dictionary uses definition as key and name as value;
            // the name is the same as definition unless specified in
            // the attribute explicitly.
            var indexDict = new SortedDictionary<string, string>();

            // Create a list of index definition strings for the class,
            // including index definitions of its base classes, eliminating
            // duplicate definitions.
            Type classType = typeof(TRecord);
            while (classType != typeof(Record))
            {
                if (classType == null)
                    throw new Exception(
                        $"Data source cannot get collection for type {typeof(TRecord).Name} " +
                        $"because it is not derived from type Record.");

                // Get class attributes with inheritance
                var classAttributes = classType.GetCustomAttributes<IndexElementsAttribute>(true);
                foreach (var classAttribute in classAttributes)
                {
                    string definition = classAttribute.Definition;
                    string name = classAttribute.Name;

                    // Validate definition and specify default value for the name if null or empty
                    if (string.IsNullOrEmpty(definition))
                        throw new Exception("Empty index definition in IndexAttribute.");
                    if (string.IsNullOrEmpty(name)) name = definition;

                    // Remove + prefix from definition if specified
                    if (definition.StartsWith("+")) definition = definition.Substring(1, definition.Length - 1);

                    if (indexDict.TryGetValue(definition, out string existingName))
                    {
                        // Already included, check that the name matches, error message otherwise
                        if (name != existingName)
                            throw new Exception(
                                $"The same index definition {definition} is provided with two different " +
                                $"custom index names {name} and {existingName} in the inheritance chain " +
                                $"for class {typeof(TRecord).Name}.");
                    }
                    else
                    {
                        // Not yet included, add
                        indexDict.Add(definition, name);
                    }
                }

                // Continue to base type
                classType = classType.BaseType;
            }

            return indexDict;
        }

        /// <summary>
        /// Parse IndexElements definition string to get an ordered
        /// list of (ElementName,SortOrder) tuples, where ElementName
        /// is name of the property for which the index is defined,
        /// and SortOrder is 1 for ascending, and -1 for descending.
        ///
        /// The parser will also validate that each element
        /// name exists in type TRecord.
        /// </summary>
        public static List<(string, int)> ParseDefinition<TRecord>(string definition)
            where TRecord : Record
        {
            var result = new List<(string, int)>();

            // Get record type to be used in element name validation
            Type recordType = typeof(TRecord);

            // Validation of the definition string
            if (string.IsNullOrEmpty(definition))
                throw new Exception($"Empty index definition string in IndexElements attribute.");
            if (definition.Contains("+"))
                throw new Exception($"Index definition string {definition}in IndexElements contains " +
                                    $"one or more + tokens. Only - but not + tokens are permitted.");

            // Parse comma separated index definition string into tokens
            // and iterate over each token
            string[] tokens = definition.Split(',');
            foreach (string token in tokens)
            {
                // Trim leading and trailing whitespace from the token
                string elementName = token.Trim();

                // Set descending sort order if the token starts from -
                int? sortOrder = null;
                if (elementName.StartsWith("-"))
                {
                    // Descending sort order
                    sortOrder = -1;

                    // Remove leading - and trim any whitespace between - and element name
                    elementName = elementName.Substring(1).TrimStart();
                }
                else
                {
                    // Ascending sort order
                    sortOrder = 1;
                }

                // Check that element name is not empty
                if (elementName.Length == 0) throw new Exception($"Empty element name in comma separated index definition string {definition}.");

                // Check that element name does not contain whitespace
                if (elementName.Contains(" ")) throw new Exception($"Element name {elementName} in comma separated index definition string {definition} contains whitespace.");

                // Check that element is present in TRecord as public property with both getter and setter
                var propertyInfo = recordType.GetProperty(elementName, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance);
                if (propertyInfo == null)
                    throw new Exception(
                        $"Property {elementName} not found in {recordType.Name} or its parents, " +
                        $"or is not a public property with both getter and setter defined.");

                // Add element and its sort order to the result
                result.Add((elementName, sortOrder.Value));
            }

            if (result.Count == 0) throw new Exception($"No index elements are found definition string {definition}.");

            return result;
        }
    }
}