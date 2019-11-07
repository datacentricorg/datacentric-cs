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
using CsvHelper.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// This class is used to project the data identifying a record
    /// when running a database query. This data includes only the
    /// record's Id, DataSet, and Key.
    ///
    /// In some cases, the projected query may not populate one or more
    /// of the RecordInfo fields.
    ///
    /// For large records, projecting to RecordInfo instead of loading
    /// the entire record makes the query significantly faster because
    /// only a small part of the record is transferred over the network.
    /// </summary>
    public sealed class RecordInfo
    {
        /// <summary>
        /// TemporalId of the record is specific to its version.
        ///
        /// For the record's history to be captured correctly, all
        /// update operations must assign a new TemporalId with the
        /// timestamp that matches update time.
        /// </summary>
        public TemporalId Id { get; set; }

        /// <summary>
        /// TemporalId of the dataset where the record is stored.
        ///
        /// For records stored in root dataset, the value of
        /// DataSet element should be TemporalId.Empty.
        /// </summary>
        public TemporalId DataSet { get; set; }

        /// <summary>
        /// String key consists of semicolon delimited primary key elements:
        ///
        /// KeyElement1;KeyElement2
        ///
        /// To avoid serialization format uncertainty, key elements
        /// can have any atomic type except Double.
        /// </summary>
        public string Key { get; set; }

        /// <summary>Return string representation of the record's key.</summary>
        public override string ToString() { return Key; }
    }
}
