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
using CsvHelper.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace DataCentric
{
    /// <summary>
    /// Contains a single entry (message) in a log.
    ///
    /// The Log record serves as the key for querying LogEntry records.
    /// To obtain the entire log, run a query for the Log element of
    /// the LogEntry record, then sort the entry records by their TemporalId.
    ///
    /// Derive from this class to provide specialized LogEntry subtypes
    /// that include additional data.
    /// </summary>
    public class LogEntry : TypedRecord<LogEntryKey, LogEntry>
    {
        /// <summary>
        /// Log for which the entry is recorded.
        ///
        /// To obtain the entire log, run a query for the Log element of
        /// the entry record, then sort the entry records by their TemporalId.
        /// </summary>
        [BsonRequired]
        public LogKey Log { get; set; }

        /// <summary>
        /// Minimal verbosity for which log entry will be displayed.
        /// </summary>
        [BsonRequired]
        public LogVerbosityEnum? Verbosity { get; set; }

        /// <summary>
        /// Short, single-line title of the log entry.
        ///
        /// Line breaks in title will be replaced by spaces when the
        /// log entry is displayed.
        /// </summary>
        [BsonRequired]
        public string Title { get; set; }

        /// <summary>
        /// Optional single-line or multi-line description of the log entry.
        ///
        /// Line breaks, whitespace and other formatting in the description
        /// will be preserved when the log entry is displayed.
        /// </summary>
        public string Description { get; set; }

        //--- METHODS

        /// <summary>
        /// Set Context property and perform validation of the record's data,
        /// then initialize any fields or properties that depend on that data.
        ///
        /// This method may be called multiple times for the same instance,
        /// possibly with a different context parameter for each subsequent call.
        ///
        /// IMPORTANT - Every override of this method must call base.Init()
        /// first, and only then execute the rest of the override method's code.
        /// </summary>
        public override void Init(IContext context)
        {
            // Initialize base
            base.Init(context);

            // We do not want to have an error inside logging code.
            // If Verbosity and Title are not specified, provide defaults
            if (Verbosity == null) Verbosity = LogVerbosityEnum.Error;
            if (string.IsNullOrEmpty(Title)) Title = "Log entry title is not specified.";
        }

        /// <summary>
        /// Returns verbosity followed by semicolon and then title
        /// with line breaks replaced by spaces, for example:
        ///
        /// Info: Sample Info Message
        /// </summary>
        public override string ToString()
        {
            string singleLineTitle = Title.Replace(Environment.NewLine, " ");
            string result = $"{Verbosity}: {singleLineTitle}";
            return result;
        }
    }
}
