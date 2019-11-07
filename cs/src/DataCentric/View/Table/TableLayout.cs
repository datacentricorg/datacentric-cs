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
    /// <summary>Indicates the presence of row and/or column headers in the table.</summary>
    public enum TableLayout
    {
        /// <summary>Empty</summary>
        Empty,

        /// <summary>Table has no headers.</summary>
        NoHeaders,

        /// <summary>Table has row headers but no column headers.</summary>
        RowHeaders,

        /// <summary>Table has column headers but no row headers.</summary>
        ColHeaders,

        /// <summary>Table has both row and column headers.</summary>
        RowAndColHeaders
    }

    /// <summary>Extension methods for TableLayout.</summary>
    public static class TableLayoutExtensions
    {
        /// <summary>Indicates that table has a corner header.</summary>
        public static bool HasCornerHeader(this TableLayout obj)
        {
            if (obj == TableLayout.Empty) throw new Exception("Table layout is empty");
            return obj == TableLayout.RowAndColHeaders;
        }

        /// <summary>
        /// Indicates that table has row headers, irrespective of
        /// whether or not it also has column headers.
        /// </summary>
        public static bool HasRowHeaders(this TableLayout obj)
        {
            if (obj == TableLayout.Empty) throw new Exception("Table layout is empty");
            return obj == TableLayout.RowHeaders || obj == TableLayout.RowAndColHeaders;
        }

        /// <summary>
        /// Indicates that table has column headers, irrespective of
        /// whether or not it also has row headers.
        /// </summary>
        public static bool HasColHeaders(this TableLayout obj)
        {
            if (obj == TableLayout.Empty) throw new Exception("Table layout is empty");
            return obj == TableLayout.ColHeaders || obj == TableLayout.RowAndColHeaders;
        }
    }
}
