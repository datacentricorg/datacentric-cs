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
using System.IO;

namespace DataCentric
{
    /// <summary>Static helper class for working with tables.</summary>
    public static class TableUtil // FIXME check use and refactor
    {
        /// <summary>(TableUtil) Returns a multi-line message where each line is separated by a blank line.</summary>
        public static string MultiLine(params string[] lines) //!! Review for possible removal
        {
            StringBuilder result = new StringBuilder();
            foreach (string line in lines)
            {
                result.Append(line);
                result.Append(StringUtil.Eols(2));
            }
            return result.ToString();
        }

        /// <summary>(TableUtil) Convert a text string to an array of lines.
        /// Accepts newlines followed by text, but ignores trailing newlines.</summary>
        public static string[] TextToLines(string multiLineText)
        {
            List<string> result = new List<string>();
            StringReader reader = new StringReader(multiLineText);
            bool emptyRowSkipped = false;
            while(true)
            {
                // Read line
                string csvLine = reader.ReadLine();

                // Exit if reached the end of multi-line string
                if (csvLine == null) break;

                // Set flag if the row is empty; if it turns out later
                // that this is not the end of file, error message.
                if (csvLine == String.Empty) { emptyRowSkipped = true; continue; }
                else if (emptyRowSkipped) throw new Exception("Empty rows can only be at the end of a CSV file but not in the middle.");

                // Append CSV line to the result
                result.Add(csvLine);
            }

            return result.ToArray();
        }

        /// <summary>(TableUtil) Convert an array of lines to a text string.
        /// Adds a trailing newline.</summary>
        public static string LinesToText(IEnumerable<string> lines)
        {
            StringBuilder result = new StringBuilder();
            foreach (string line in lines)
            {
                result.AppendLine(line);
            }
            return result.ToString();
        }

        /// <summary>(TableUtil) Convert byte array to text assuming UTF-8 encoding.</summary>
        public static string BytesToText(byte[] bytes)
        {
            if (bytes != null && bytes.Length != 0)
            {
                // This method has more lines of code compared to Encoding.UTF8.GetString,
                // however it will correctly recognize and remove UTF-8 BOM
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    using (StreamReader streamReader = new StreamReader(memoryStream))
                    {
                        string result = streamReader.ReadToEnd();
                        return result;
                    }
                }
            }
            else
            {
                return String.Empty;
            }
        }
    }
}
