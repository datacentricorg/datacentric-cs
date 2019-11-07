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
using System.CodeDom.Compiler;
using System.IO;

namespace DataCentric
{
    /// <summary>
    /// Abstract base class of Log implementations that convert entries to text.
    /// </summary>
    public abstract class TextLog : Log
    {
        private readonly string indentString_ = new String(' ', 4);
        private readonly string[] lineSeparators_ = new string[] {"\r\n", "\r", "\n"};
        protected TextWriter textWriter_;

        //--- METHODS

        /// <summary>
        /// Releases resources and calls base.Dispose().
        ///
        /// This method will not be called by the garbage collector.
        /// It will only be executed if:
        ///
        /// * This class implements IDisposable; and
        /// * The class instance is created through the using clause
        ///
        /// IMPORTANT - Every override of this method must call base.Dispose()
        /// after executing its own code.
        /// </summary>
        public override void Dispose()
        {
            textWriter_.Close();
            textWriter_.Dispose();

            // Dispose base
            base.Dispose();
        }

        /// <summary>Flush data to permanent storage.</summary>
        public override void Flush()
        {
            textWriter_.Flush();
        }

        /// <summary>
        /// Publish the specified entry to the log if log verbosity
        /// is the same or high as entry verbosity.
        ///
        /// When log entry data is passed to this method, only the following
        /// elements are required:
        ///
        /// * Verbosity
        /// * Title (should not have line breaks; if found will be replaced by spaces)
        /// * Description (line breaks and formatting will be preserved)
        ///
        /// The remaining fields of LogEntry will be populated if the log
        /// entry is published to a data source. They are not necessary if the
        /// log entry is published to a text log.
        ///
        /// In a text log, the first line of each log entry is Verbosity
        /// followed by semicolon separator and then Title of the log entry.
        /// Remaining lines are Description of the log entry recorded with
        /// 4 space indent but otherwise preserving its formatting.
        ///
        /// Example:
        ///
        /// Info: Sample Title
        ///     Sample Description Line 1
        ///     Sample Description Line 2
        /// </summary>
        public override void Publish(LogEntry logEntry)
        {
            // Do not record the log entry if entry verbosity exceeds log verbosity
            // Record all entries if log verbosity is not specified
            if (logEntry.Verbosity <= Verbosity)
            {
                // Title should not have line breaks; if found will be replaced by spaces
                string titleWithNoSpaces = logEntry.Title.Replace(Environment.NewLine, " ");
                string formattedTitle = $"{logEntry.Verbosity}: {titleWithNoSpaces}";
                textWriter_.WriteLine(formattedTitle);

                // Skip if Description is not specified
                if (!string.IsNullOrEmpty(logEntry.Description))
                {
                    // Split the description into lines
                    string[] descriptionLines =
                        logEntry.Description.Split(lineSeparators_, StringSplitOptions.None);

                    // Write lines with indent and remove the trailing blank line if any
                    int descriptionLineCount = descriptionLines.Length;
                    for (int i = 0; i < descriptionLineCount; ++i)
                    {
                        string descriptionLine = descriptionLines[i];
                        if (string.IsNullOrEmpty(descriptionLine))
                        {
                            if (i < descriptionLineCount - 1)
                            {
                                // Write empty line unless the empty token is last, in which
                                // case it represents the trailing EOL and including it would
                                // create in a trailing empty line not present in the original
                                // log message
                                textWriter_.WriteLine();
                            }
                        }
                        else
                        {
                            // Write indent followed by description line
                            textWriter_.Write(indentString_);
                            textWriter_.WriteLine(descriptionLine);
                        }
                    }
                }
            }
        }
    }
}
