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
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Log record implements ILog interface for recording log
    /// entries in a data source. Each log entry is a separate
    /// record.
    ///
    /// The log record serves as the key for querying log entries.
    /// To obtain the entire log, run a query for the Log element
    /// of the LogEntry record, then sort the entry records by
    /// their TemporalId.
    /// </summary>
    public abstract class Log : TypedRecord<LogKey, Log>, ILog
    {
        /// <summary>Unique log name.</summary>
        [BsonRequired]
        public string LogName { get; set; }

        /// <summary>
        /// Minimal verbosity for which log entry will be displayed.
        /// </summary>
        public LogVerbosityEnum? Verbosity { get; set; }

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

            if (Verbosity == null)
            {
                // If verbosity is null, set to Error
                Verbosity = LogVerbosityEnum.Error;
            }
        }

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
        public virtual void Dispose()
        {
            // Uncomment except in root class of the hierarchy
            // base.Dispose();
        }

        /// <summary>Flush data to permanent storage.</summary>
        public abstract void Flush();

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
        public abstract void Publish(LogEntry logEntry);
    }
}
