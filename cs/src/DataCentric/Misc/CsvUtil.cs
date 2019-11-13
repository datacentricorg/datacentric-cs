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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace DataCentric
{
    /// <summary>Static helper class for working with CSV files.</summary>
    public static class CsvUtil
    {
        /// <summary>
        /// Convert a text string to an array of lines.
        /// 
        /// Accepts empty newlines followed by non-empty newlines,
        /// but ignores trailing newlines.
        /// </summary>
        public static string[] TextToLines(string multiLineText)
        {
            List<string> result = new List<string>();
            StringReader reader = new StringReader(multiLineText);
            bool emptyRowSkipped = false;
            while (true)
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

        /// <summary>
        /// Convert an array of lines to a text string.
        /// 
        /// Adds a trailing newline.
        /// </summary>
        public static string LinesToText(IEnumerable<string> lines)
        {
            StringBuilder result = new StringBuilder();
            foreach (string line in lines)
            {
                result.AppendLine(line);
            }
            return result.ToString();
        }

        /// <summary>Convert byte array to text string assuming UTF-8 encoding.</summary>
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

        /// <summary>
        /// Convert a text line to CSV tokens.
        ///
        /// Uses quote symbol to escape strings containing CSV separator.
        /// Two quote symbols inside an escaped string in a row are converted to one.
        /// This function uses current locale settings for parsing.
        /// </summary>
        public static string[] LineToTokens(string csvLine)
        {
            List<string> result = new List<string>();

            // Check if a single line is provided
            if (csvLine.Contains(StringUtil.Eol)) throw new Exception($"Multi-line string encountered in CSV file: {csvLine}");

            char separator = LocaleSettings.ListSeparator;
            char quote =  LocaleSettings.QuoteSymbol;

            bool isInsideQuotes = false;
            char[] chars = csvLine.ToCharArray();
            StringBuilder token = new StringBuilder();
            for (int i = 0; i < chars.Length; ++i)
            {
                char c = chars[i];
                bool isNextCharQuote = (c == quote) && (i < chars.Length - 1) && (chars[i + 1] == quote);

                if (isInsideQuotes)
                {
                    if (c == quote)
                    {
                        if (isNextCharQuote)
                        {
                            // Add one quote to token, skip next character and continue
                            token.Append(c);
                            i++;
                        }
                        else
                        {
                            // Exit quote mode
                            isInsideQuotes = false;
                        }
                    }
                    else
                    {
                        // If not a quote, continue
                        token.Append(c);
                    }
                }
                else
                {
                    if (c == quote)
                    {
                        // Exit enter quote mode
                        isInsideQuotes = true;
                    }
                    else if (c == separator)
                    {
                        // Close token
                        result.Add(token.ToString());
                        token = new StringBuilder();
                    }
                    else
                    {
                        // Otherwise continue
                        token.Append(c);
                    }
                }
            }

            // Close final token
            if (token != null)
            {
                result.Add(token.ToString());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Convert an array of CSV tokens to a text line.
        ///
        /// Uses quote symbol to escape strings containing CSV separator.
        /// Two quote symbols inside an escaped string in a row are converted to one.
        /// This function uses locale-specific settings.
        /// </summary>
        public static string TokensToLine(IEnumerable<string> tokens) //!! Deprecated
        {
            StringBuilder result = new StringBuilder();

            string listSeparator =  LocaleSettings.ListSeparator.ToString();
            string singleQuote =  LocaleSettings.QuoteSymbol.ToString();
            string doubleQuote = singleQuote + singleQuote;

            int tokenCount = 0;
            foreach (string token in tokens)
            {
                // Add list separator except in front of the first symbol
                if (tokenCount++ > 0) result.Append( LocaleSettings.ListSeparator);

                // Check that there is no newline
                if (token.Contains(StringUtil.Eol)) throw new Exception($"Multi-line string encountered in CSV file: {token}");

                // If there is a CSV separator, escape with quotes
                if (token.Contains(listSeparator))
                {
                    // Escape quotes
                    string escapedToken = token.Replace(singleQuote, doubleQuote);

                    // Escape separator
                    escapedToken = String.Concat(singleQuote, escapedToken, singleQuote);

                    // Add to result
                    result.Append(escapedToken);
                }
                else
                {
                    // Otherwise appent without changes
                    result.Append(token);
                }
            }

            return result.ToString();
        }


        /// <summary>
        /// Escape CSV separator if encountered in argument token.
        ///
        /// Returns the argument surrounded by quotes, if it contains
        /// the CSV separator.
        /// </summary>
        private static string EscapeCsvSeparator(string token)
        {
            string listSeparator = LocaleSettings.ListSeparator.ToString();
            char quoteSymbol = LocaleSettings.QuoteSymbol;
            string quoteString = quoteSymbol.ToString();
            string repeatedQuoteString = quoteString + quoteString;

            // Check that there is no newline
            if (token.Contains(StringUtil.Eol)) throw new Exception($"Multi-line string encountered in CSV file: {token}");

            // Count quote (") symbols in string, error message if not an even number
            int quoteCount = token.Count(p => p == quoteSymbol);
            if (quoteCount % 2 != 0) throw new Exception($"Odd number of quote ({quoteSymbol}) symbols in token: {token}");

            // If there is a CSV separator, perform additional processing to escape it with quotes
            if (token.Contains(listSeparator))
            {
                if (token.StartsWith(quoteString) && token.EndsWith(quoteString))
                {
                    // Already starts and ends with quotes, do nothing
                    return token;
                }
                else
                {
                    // Escape each single instance of quote with repeated quote
                    string result = token.Replace(quoteString, repeatedQuoteString);

                    // Then escape list separator with a single instance of quote
                    result = String.Concat(quoteString, result, quoteString);
                    return result;
                }
            }
            else
            {
                // No need to escape the list separator, return the original quote
                return token;
            }
        }
    }
}
