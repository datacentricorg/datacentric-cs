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

namespace DataCentric
{
    /// <summary>
    /// Provides the ability to change data associated with the dataset
    /// without changing the dataset record, which is immutable in a
    /// temporal data source.
    ///
    /// The reason dataset record is immutable is that any change to the
    /// the dataset record in a temporal data source results in creation
    /// of a record with new TemporalId, which is treated as a new dataset.
    ///
    /// The DataSetDetail record uses TemporalId of the referenced dataset
    /// as its primary key. It is located in the parent of the dataset
    /// record to which it applies, rather than inside that record, so it
    /// is not affected by its own settings.
    /// </summary>
    [BsonSerializer(typeof(BsonKeySerializer<DataSetDetailKey>))]
    public sealed class DataSetDetailKey : TypedKey<DataSetDetailKey, DataSetDetail>
    {
        /// <summary>
        /// TemporalId of the referenced dataset.
        /// </summary>
        public TemporalId DataSetId { get; set; }
    }
}
