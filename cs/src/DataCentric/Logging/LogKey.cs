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
using NodaTime;

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
    [BsonSerializer(typeof(BsonKeySerializer<LogKey>))]
    public sealed class LogKey : TypedKey<LogKey, Log>
    {
        /// <summary>Unique log name.</summary>
        public string LogName { get; set; }
    }
}
