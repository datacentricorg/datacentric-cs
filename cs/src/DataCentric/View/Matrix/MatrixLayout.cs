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
    public enum MatrixLayout
    {
        /// <summary>
        /// Indicates that enum value is not set.
        /// 
        /// In programming languages where enum defaults to the first item when
        /// not set, making Empty the first item prevents unintended assignment
        /// of a meaningful value.
        /// </summary>
        Empty,

        /// <summary>Matrix has no headers.</summary>
        NoHeaders,

        /// <summary>Matrix has row headers but no column headers.</summary>
        RowHeaders,

        /// <summary>Matrix has column headers but no row headers.</summary>
        ColHeaders,

        /// <summary>Matrix has both row and column headers.</summary>
        RowAndColHeaders
    }

    /// <summary>Extension methods for MatrixLayout.</summary>
    public static class MatrixLayoutExtensions
    {
        /// <summary>Indicates that table has a corner header.</summary>
        public static bool HasCornerHeader(this MatrixLayout obj)
        {
            if (obj == MatrixLayout.Empty) throw new Exception("Matrix layout is empty");
            return obj == MatrixLayout.RowAndColHeaders;
        }

        /// <summary>
        /// Indicates that table has row headers, irrespective of
        /// whether or not it also has column headers.
        /// </summary>
        public static bool HasRowHeaders(this MatrixLayout obj)
        {
            if (obj == MatrixLayout.Empty) throw new Exception("Matrix layout is empty");
            return obj == MatrixLayout.RowHeaders || obj == MatrixLayout.RowAndColHeaders;
        }

        /// <summary>
        /// Indicates that table has column headers, irrespective of
        /// whether or not it also has row headers.
        /// </summary>
        public static bool HasColHeaders(this MatrixLayout obj)
        {
            if (obj == MatrixLayout.Empty) throw new Exception("Matrix layout is empty");
            return obj == MatrixLayout.ColHeaders || obj == MatrixLayout.RowAndColHeaders;
        }
    }
}
