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
    /// Data source is a logical concept similar to database
    /// that can be implemented for a document DB, relational DB,
    /// key-value store, or filesystem.
    ///
    /// Data source API provides the ability to:
    ///
    /// (a) store and query datasets;
    /// (b) store records in a specific dataset; and
    /// (c) query record across a group of datasets.
    ///
    /// This record is stored in root dataset.
    /// </summary>
    [BsonSerializer(typeof(BsonKeySerializer<DataSourceKey>))]
    public class DataSourceKey : TypedKey<DataSourceKey, DataSource>
    {
        /// <summary>Unique data source name.</summary>
        public string DataSourceName { get; set; }

        /// <summary>
        /// By convention, Cache is the name of the Operational Data Store (ODS).
        /// </summary>
        public static DataSourceKey Cache { get; } = new DataSourceKey() { DataSourceName = "Cache" };

        /// <summary>
        /// By convention, Master is the name of the Master Data Store (MDS).
        /// </summary>
        public static DataSourceKey Master { get; } = new DataSourceKey() { DataSourceName = "Master" };
    }
}
