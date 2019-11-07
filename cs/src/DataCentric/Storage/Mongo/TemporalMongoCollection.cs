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
using MongoDB.Driver;

namespace DataCentric
{
    /// <summary>
    /// Wrapper around native collection for temporal MongoDB data source.
    ///
    /// This object holds two collection references - one for the base
    /// type of all records and the other for the record type specified
    /// as generic parameter.
    ///
    /// The need to hold two collection arises from the requirement
    /// that query for a derived type takes into account that another
    /// record with the same key and later dataset or object timestamp
    /// may exist. For this reason, the typed collection is used for
    /// LINQ constraints and base collection is used to iterate over
    /// objects.
    /// </summary>
    public class TemporalMongoCollection<TRecord>
        where TRecord : Record
    {
        /// <summary>
        /// Create from data source, base collection, and typed collection objects.
        ///
        /// This object should be constructed inside a data source. It should not
        /// be used by other classes directly.
        /// </summary>
        public TemporalMongoCollection(
            TemporalMongoDataSource dataSource,
            IMongoCollection<Record> baseCollection,
            IMongoCollection<TRecord> typedCollection)
        {
            DataSource = dataSource;
            BaseCollection = baseCollection;
            TypedCollection = typedCollection;
        }

        /// <summary>Interface to the data source.</summary>
        public TemporalMongoDataSource DataSource { get; }

        /// <summary>Collection for the base record type.</summary>
        public IMongoCollection<Record> BaseCollection { get; }

        /// <summary>Collection for the generic parameter type TRecord.</summary>
        public IMongoCollection<TRecord> TypedCollection { get; }
    }
}
