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
    /// <summary>
    /// Table of atomic value types that can be resized by adding rows.
    ///
    /// This matrix uses Variant.Parse to convert strings
    /// to objects of the specified type. The supported types
    /// are the same as the types supported by Variant.
    ///
    /// The Layout property indicates if row, column, or both types of
    /// headers are present. Column value types indicate the type of
    /// values in the corresponding column (excluding row headers
    /// column whose type is always string).
    ///
    /// Each header is a string that may optionally use dot delimiter
    /// to represent hierarchical structure.
    /// </summary>
    public class ValueTable : Table<object>
    {
        /// <summary>
        /// Create from multi-line CSV text using the specified matrix
        /// layout and a single value type that will be used for all columns
        /// except the column of row headers (if present).
        ///
        /// The specified value type is not used for row and column headers
        /// which are always dot delimited strings.
        /// </summary>
        public void ParseCsv(TableLayout layout, AtomicType valueType, string csvText)
        {
            // Create an array of parsers of size one
            // The function taking the array will use
            // its last (and only) element for all of
            // the data columns
            var valueTypes = new AtomicType[] {valueType};

            // Parse and populate the array of column value types
            ParseCsv(layout, valueTypes, csvText);
        }

        /// <summary>
        /// Populate by parsing multi-line CSV text using the specified
        /// matrix layout and an array of column parser functions.
        ///
        /// If the data has more columns than the array of parsers,
        /// the last parser in the array is used for all additional
        /// columns. This permits ingesting CSV files with an unknown
        /// number of value columns of the same type, after an initial
        /// set of category columns that have other types.
        ///
        /// The specified parser is not used for row and column headers
        /// which are always dot delimited strings.
        /// </summary>
        public void ParseCsv(TableLayout layout, AtomicType[] colTypes, string csvText)
        {
            // Create and populate an array of column parser functions
            var parsers = new Func<string, object>[colTypes.Length];
            for (int i = 0; i < parsers.Length; i++)
            {
                // It is important to copy and pass ValueType by value
                // rather than as array and element index, because the
                // array may change by the time the expression is invoked
                AtomicType colType = colTypes[i];
                parsers[i] = value => Variant.Parse(colType, value).Value;
            }

            // Parse CSV text
            ParseCsv(layout, parsers, csvText);

            // Populate the array of column value types using the actual matrix size,
            // padding with the last value of the argument array
            ColTypes = new AtomicType[ColCount];
            for (int i = 0; i < ColTypes.Length; i++)
            {
                if (i < colTypes.Length) ColTypes[i] = colTypes[i];
                else ColTypes[i] = colTypes[colTypes.Length - 1];
            }
        }

        /// <summary>
        /// Array of value types for the matrix columns.
        ///
        /// Excludes the type of row headers which are
        /// always dot delimited strings.
        /// </summary>
        public AtomicType[] ColTypes { get; private set; }
    }
}
