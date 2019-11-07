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
using System.IO;

namespace DataCentric
{
    /// <summary>
    /// Implementation of ILog that writes LogEntry records into
    /// the data source and dataset where Log record is located.
    /// </summary>
    public sealed class DataLog : Log
    {
        //--- METHODS

        /// <summary>Flush data to permanent storage.</summary>
        public override void Flush()
        {
            // Do nothing
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
                // Record the log entry by saving it into the data source
                // and dataset where Log record is located.
                Context.SaveOne(logEntry);
            }
        }
    }
}
