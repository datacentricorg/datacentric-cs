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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Records a single entry in a log.
    ///
    /// The log record serves as the key for querying log entries.
    /// To obtain the entire log, run a query for the Log element of
    /// the entry record, then sort the entry records by their ObjectId.
    ///
    /// Derive from this class to provide specialized log entry types
    /// that include additional data.
    /// </summary>
    [BsonSerializer(typeof(BsonKeySerializer<LogEntryKey>))]
    public sealed class LogEntryKey : TypedKey<LogEntryKey, LogEntryData>
    {
        /// <summary>
        /// Defining element Id here includes the record's ObjectId
        /// in its key. Because ObjectId of the record is specific
        /// to its version, this is equivalent to using an auto-
        /// incrementing column as part of the record's primary key
        /// in a relational database.
        ///
        /// For the record's history to be captured correctly, all
        /// update operations must assign a new ObjectId with the
        /// timestamp that matches update time.
        /// </summary>
        public ObjectId Id { get; set; }
    }
}