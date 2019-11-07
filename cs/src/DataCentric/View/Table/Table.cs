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
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace DataCentric
{
    /// <summary>
    /// Table of type T that can be resized by adding rows.
    ///
    /// The Layout property indicates if row, column, or both types of
    /// headers are present. Each header is a string that may optionally
    /// use dot delimiter to represent hierarchical structure.
    /// </summary>
    public class Table<T>
    {
        private int rowCount_;
        private int colCount_;
        private List<T> values_;

        /// <summary>
        /// Create from multi-line CSV text using the specified matrix
        /// layout and a single parser that will be used for all columns
        /// except the column of row headers (if present).
        ///
        /// The specified parser is not used for row and column headers
        /// which are always dot delimited strings.
        /// </summary>
        public void ParseCsv(TableLayout layout, Func<string, T> parser, string csvText)
        {
            // Create an array of parsers of size one
            // The function taking the array will use
            // its last (and only) element for all of
            // the data columns
            var parsers = new Func<string, T>[] {parser};
            ParseCsv(layout, parsers, csvText);
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
        public void ParseCsv(TableLayout layout, Func<string, T>[] colParsers, string csvText)
        {
            if (layout == TableLayout.Empty) throw new Exception("Table layout passed to ParseCsv method is empty");
            Layout = layout;

            // Parse into a list of text lines
            string[] csvLines = TableUtil.TextToLines(csvText);

            // Parse each line into tokens, keeping track of maximum
            // size which will determine the matrix size
            int rowCount = csvLines.Length;
            List<string[]> parsedRows = new List<string[]>();
            int colCount = 0;
            foreach (string csvLine in csvLines)
            {
                string[] tokens = CsvUtil.LineToTokens(csvLine);
                if (colCount < tokens.Length) colCount = tokens.Length;
                parsedRows.Add(tokens);
            }

            int colOffset = 0;
            if (layout.HasRowHeaders())
            {
                // First column is row headers, data has one less column
                colOffset = 1;
                colCount--;
            }

            int rowOffset = 0;
            if (layout.HasColHeaders())
            {
                // First row is column headers, data has one less row
                rowOffset = 1;
                rowCount--;
            }

            // Resize
            Resize(layout, rowCount, colCount);

            // Parse column headers if present
            if (rowOffset != 0)
            {
                string[] rowTokens = parsedRows[0];

                // Populate corner header if present
                if (colOffset != 0) CornerHeader = rowTokens[0];

                // Populate column headers if present
                for (int colIndex = 0; colIndex < colCount; ++colIndex)
                {
                    string token = rowTokens[colOffset + colIndex];
                    ColHeaders[colIndex] = token;
                }
            }

            for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex)
            {
                string[] rowTokens = parsedRows[rowOffset + rowIndex];

                // Set row header if present
                if (colOffset != 0) RowHeaders[rowIndex] = rowTokens[0];

                // Set row values
                for (int colIndex = 0; colIndex < colCount; ++colIndex)
                {
                    // If column index is outside the range of column parsers array,
                    // take the last element of the array instead. This permits
                    // parsing CSV data with unknown number of columns where trailing
                    // columns have the same type
                    int parserColIndex = colIndex < colParsers.Length ? colIndex : colParsers.Length - 1;
                    Func<string, T> colParser = colParsers[parserColIndex];

                    string token = rowTokens[colOffset + colIndex];

                    values_[LinearIndex(rowIndex, colIndex)] = colParser(token);
                }
            }
        }

        /// <summary>Indicates which headers are present.</summary>
        public TableLayout Layout { get; set; }

        /// <summary>Number of matrix rows (excluding header rows, if any).</summary>
        public int RowCount
        {
            get { return rowCount_; }
            set
            {
                if (rowCount_ != value)
                {
                    if (rowCount_ < value)
                    {
                        // Resize values array if column count is also set
                        if (colCount_ > 0)
                        {
                            if (values_ != null) values_.AddRange(Enumerable.Repeat(default(T), (value-rowCount_) * colCount_));
                            values_ = Enumerable.Repeat(default(T), value * colCount_).ToList();
                        }

                        rowCount_ = value;
                    }
                    else throw new Exception(
                        $"New row count {value} cannot be less than the previous row count {rowCount_} " +
                            $"as this would lead to the loss of previously set values.");
                }
            }
        }

        /// <summary>Number of matrix columns (excluding header columns, if any).</summary>
        public int ColCount
        {
            get { return colCount_; }
            set
            {
                if (colCount_ != value)
                {
                    if (colCount_ == 0)
                    {
                        colCount_ = value;

                        // Resize values array if row count is also set
                        if (rowCount_ > 0)
                        {
                            if (values_ != null) throw new Exception("Values array exists before column count is set.");
                            values_ = Enumerable.Repeat(default(T), rowCount_*colCount_).ToList();
                        }
                    }
                    else throw new Exception("Column count can be set only if the previous value is 0.");
                }
            }
        }

        /// <summary>
        /// Corner header is a special type of header that is present in top left corner
        /// of the matrix when both row and column are present.
        ///
        /// The corner header is a string that may optionally use dot delimiter
        /// to represent hierarchical structure.
        ///
        /// Returns null if the matrix does not have the corner header.</summary>
        public string CornerHeader { get; set; }

        /// <summary>
        /// Array of row headers.
        ///
        /// Each header is a string that may optionally use dot delimiter
        /// to represent hierarchical structure.
        ///
        /// Returns null if the matrix does not have row headers.</summary>
        public string[] RowHeaders { get; set; }

        /// <summary>
        /// Array of column headers.
        ///
        /// Each header is a string that may optionally use dot delimiter
        /// to represent hierarchical structure.
        ///
        /// Returns null if the matrix does not have column headers.</summary>
        public string[] ColHeaders { get; set; }

        /// <summary>Access elements by index.</summary>
        public T this[int rowIndex, int colIndex]
        {
            get { return values_[LinearIndex(rowIndex, colIndex)]; }
            set { values_[LinearIndex(rowIndex, colIndex)] = value; }
        }

        /// <summary>
        /// Create data containers inside the matrix and populate the values
        /// with default(T).
        ///
        /// This method also creates the row and column headers with the
        /// correct size, if the respective header is specified in the layout.
        /// </summary>
        public void Resize(TableLayout layout, int rowCount, int colCount)
        {
            Layout = layout;
            RowCount = rowCount;
            ColCount = colCount;

            if (layout.HasRowHeaders()) RowHeaders = new string[rowCount];
            if (layout.HasColHeaders()) ColHeaders = new string[colCount];
        }

        /// <summary>Validate dimensions and that the presence of headers matches the layout.</summary>
        public void Validate()
        {
            // Table can have no rows but it must have at least one column
            if (ColCount == 0) throw new Exception($"Table has no columns");

            // Validate row headers
            if (Layout.HasRowHeaders())
            {
                // If layout specifies row headers, they must have be present
                if (RowHeaders == null || (RowCount != 0 && RowHeaders.Length == 0))
                    throw new Exception($"Table has layout {Layout} but no row headers.");

                // If layout specifies row headers, they must have non-empty value in each row
                for (int i = 0; i < RowHeaders.Length; i++)
                {
                    if (string.IsNullOrEmpty(RowHeaders[i]))
                        throw new Exception($"Row header with zero-based index {i} is null or empty.");
                }
            }
            else
            {
                // If layout specifies no row headers, RowHeaders must be null
                if(RowHeaders != null)
                    throw new Exception($"Table has layout {Layout} but row headers are present.");
            }

            // Validate column headers
            if (Layout.HasColHeaders())
            {
                // If layout specifies column headers, they must have be present
                if (ColHeaders == null || (ColCount != 0 && ColHeaders.Length == 0))
                    throw new Exception($"Table has layout {Layout} but no column headers.");

                // If layout specifies column headers, they must have non-empty value in each column
                for (int i = 0; i < ColHeaders.Length; i++)
                {
                    if (string.IsNullOrEmpty(ColHeaders[i]))
                        throw new Exception($"Col header with zero-based index {i} is null or empty.");
                }
            }
            else
            {
                // If layout specifies no column headers, ColHeaders must be null
                if(ColHeaders != null)
                    throw new Exception($"Table has layout {Layout} but column headers are present.");
            }
        }

        /// <summary>
        /// Convert to a multi-line CSV text with column and/or row headers.
        ///
        /// Escapes separator with quotes if necessary.
        /// </summary>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            // Validate table before serializing it to CSV string
            Validate();

            // Add column headers, if any
            if (ColHeaders != null)
            {
                IEnumerable<string> firstRowTokens = null;
                if (RowHeaders != null)
                {
                    // Prepend corner value if row headers are also present
                    firstRowTokens = new string[1] { CornerHeader }.Concat(ColHeaders);
                }
                else
                {
                    // No corner value
                    firstRowTokens = ColHeaders;
                }
                string firstRowString = CsvUtil.TokensToLine(firstRowTokens);
                result.AppendLine(firstRowString);
            }

            int colOffset = RowHeaders != null ? 1 : 0;
            for (int rowIndex = 0; rowIndex < RowCount; ++rowIndex)
            {
                string[] tokens = new string[colOffset + ColCount];

                if (colOffset != 0)
                {
                    // Add row header, if present
                    tokens[0] = RowHeaders[rowIndex];
                }

                for (int colIndex = 0; colIndex < ColCount; ++colIndex)
                {
                    // Add values with offset for row header, if present
                    tokens[colOffset + colIndex] = this[rowIndex, colIndex].AsString();
                }

                // Convert to CSV and append
                string csvLine = CsvUtil.TokensToLine(tokens);
                result.AppendLine(csvLine);
            }

            return result.ToString();
        }

        /// <summary>
        /// Get index of table element in its linear data array
        /// after checking that element coordinates are in bounds.
        /// </summary>
        private int LinearIndex(int rowIndex, int colIndex)
        {
            if (rowIndex >= RowCount)
                throw new Exception($"Row index {rowIndex} is out of bounds for row count {RowCount}.");
            if (colIndex >= ColCount)
                throw new Exception($"Column index {colIndex} is out of bounds for column count {ColCount}.");

            int result = rowIndex * ColCount + colIndex;
            return result;
        }
    }
}
