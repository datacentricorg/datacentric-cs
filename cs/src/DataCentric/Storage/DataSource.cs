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
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

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
    [Pinned]
    public abstract class DataSource : TypedRecord<DataSourceKey, DataSource>, IDisposable
    {
        /// <summary>Unique data source name.</summary>
        [BsonRequired]
        public string DataSourceName { get; set; }

        /// <summary>
        /// Environment type enumeration.
        ///
        /// Some API functions are restricted based on the environment type.
        /// </summary>
        [BsonRequired]
        public EnvType EnvType { get; set; }

        /// <summary>
        /// The meaning of environment group depends on the environment type.
        ///
        /// * For PROD, UAT, and DEV environment types, environment group
        ///   identifies the endpoint.
        ///
        /// * For USER environment type, environment group is user alias.
        ///
        /// * For TEST environment type, environment group is the name of
        ///   the unit test class (test fixture).
        /// </summary>
        [BsonRequired]
        public string EnvGroup { get; set; }

        /// <summary>
        /// The meaning of environment name depends on the environment type.
        ///
        /// * For PROD, UAT, DEV, and USER environment types, it is the
        ///   name of the user environment selected in the client.
        ///
        /// * For TEST environment type, it is the test method name.
        /// </summary>
        [BsonRequired]
        public string EnvName { get; set; }

        /// <summary>
        /// Specifies the default method of record or dataset
        /// versioning.
        ///
        /// This value can be overridden for specific record types
        /// via an attribute.
        /// </summary>
        [BsonRequired]
        public VersioningMethod? VersioningMethod { get; set; }

        /// <summary>
        /// Use this flag to mark data source as readonly.
        ///
        /// Data source may also be readonly because CutoffTime is set.
        /// </summary>
        public bool? ReadOnly { get; set; }

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
        public virtual void Dispose()
        {
            // Uncomment except in root class of the hierarchy
            // base.Dispose();
        }

        /// <summary>Flush data to permanent storage.</summary>
        public abstract void Flush();

        /// <summary>
        /// The returned TemporalIds have the following order guarantees:
        ///
        /// * For this data source instance, to arbitrary resolution; and
        /// * Across all processes and machines, to one second resolution
        ///
        /// One second resolution means that two TemporalIds created within
        /// the same second by different instances of the data source
        /// class may not be ordered chronologically unless they are at
        /// least one second apart.
        /// </summary>
        public abstract TemporalId CreateOrderedTemporalId();

        /// <summary>
        /// Load record by its TemporalId.
        ///
        /// Return null if there is no record for the specified TemporalId;
        /// however an exception will be thrown if the record exists but
        /// is not derived from TRecord.
        /// </summary>
        public abstract TRecord LoadOrNull<TRecord>(TemporalId id)
            where TRecord : Record;

        /// <summary>
        /// Load record by string key from the specified dataset or
        /// its list of imports. The lookup occurs first in descending
        /// order of dataset TemporalIds, and then in the descending
        /// order of record TemporalIds within the first dataset that
        /// has at least one record. Both dataset and record TemporalIds
        /// are ordered chronologically to one second resolution,
        /// and are unique within the database server or cluster.
        ///
        /// The root dataset has empty TemporalId value that is less
        /// than any other TemporalId value. Accordingly, the root
        /// dataset is the last one in the lookup order of datasets.
        ///
        /// The first record in this lookup order is returned, or null
        /// if no records are found or if DeletedRecord is the first
        /// record.
        ///
        /// Return null if there is no record for the specified TemporalId;
        /// however an exception will be thrown if the record exists but
        /// is not derived from TRecord.
        /// </summary>
        public abstract TRecord LoadOrNull<TKey, TRecord>(TypedKey<TKey, TRecord> key, TemporalId loadFrom)
            where TKey : TypedKey<TKey, TRecord>, new()
            where TRecord : TypedRecord<TKey, TRecord>;

        /// <summary>
        /// Get query for the specified type.
        ///
        /// After applying query parameters, the lookup occurs first in
        /// descending order of dataset TemporalIds, and then in the descending
        /// order of record TemporalIds within the first dataset that
        /// has at least one record. Both dataset and record TemporalIds
        /// are ordered chronologically to one second resolution,
        /// and are unique within the database server or cluster.
        ///
        /// The root dataset has empty TemporalId value that is less
        /// than any other TemporalId value. Accordingly, the root
        /// dataset is the last one in the lookup order of datasets.
        ///
        /// Generic parameter TRecord is not necessarily the root data type;
        /// it may also be a type derived from the root data type.
        /// </summary>
        public abstract IQuery<TRecord> GetQuery<TRecord>(TemporalId loadFrom)
            where TRecord : Record;

        /// <summary>
        /// Save multiple records to the specified dataset. After the method exits,
        /// for each record the property record.DataSet will be set to the value of
        /// the saveTo parameter.
        ///
        /// All Save methods ignore the value of record.DataSet before the
        /// Save method is called. When dataset is not specified explicitly,
        /// the value of dataset from the context, not from the record, is used.
        /// The reason for this behavior is that the record may be stored from
        /// a different dataset than the one where it is used.
        ///
        /// This method guarantees that TemporalIds of the saved records will be in
        /// strictly increasing order.
        /// </summary>
        public abstract void SaveMany<TRecord>(IEnumerable<TRecord> records, TemporalId saveTo)
            where TRecord : Record;

        /// <summary>
        /// Write a DeletedRecord in deleteIn dataset for the specified key
        /// instead of actually deleting the record. This ensures that
        /// a record in another dataset does not become visible during
        /// lookup in a sequence of datasets.
        ///
        /// To avoid an additional roundtrip to the data store, the delete
        /// marker is written even when the record does not exist.
        /// </summary>
        public abstract void Delete<TKey, TRecord>(TypedKey<TKey, TRecord> key, TemporalId deleteIn)
            where TKey : TypedKey<TKey, TRecord>, new()
            where TRecord : TypedRecord<TKey, TRecord>;

        /// <summary>
        /// Permanently deletes (drops) the database with all records
        /// in it without the possibility to recover them later.
        ///
        /// This method should only be used to free storage. For
        /// all other purposes, methods that preserve history should
        /// be used.
        ///
        /// ATTENTION - THIS METHOD WILL DELETE ALL DATA WITHOUT
        /// THE POSSIBILITY OF RECOVERY. USE WITH CAUTION.
        /// </summary>
        public abstract void DeleteDb();

        //--- METHODS

        /// <summary>
        /// Get TemporalId of the dataset with the specified name.
        ///
        /// All of the previously requested dataSetIds are cached by
        /// the data source. To load the latest version of the dataset
        /// written by a separate process, clear the cache first by
        /// calling DataSource.ClearDataSetCache() method.
        ///
        /// Returns null if not found.
        /// </summary>
        public abstract TemporalId? GetDataSetOrNull(string dataSetName);

        /// <summary>
        /// Save new version of the dataset.
        ///
        /// This method sets Id element of the argument to be the
        /// new TemporalId assigned to the record when it is saved.
        /// The timestamp of the new TemporalId is the current time.
        ///
        /// This method updates in-memory cache to the saved dataset.
        /// </summary>
        public abstract void SaveDataSet(DataSet dataSetRecord);
    }

    /// <summary>Extension methods for DataSource.</summary>
    public static class DataSourceExtensions
    {
        /// <summary>
        /// Load record by its TemporalId.
        ///
        /// Error message if there is no record for the specified TemporalId,
        /// or if the record exists but is not derived from TRecord.
        /// </summary>
        public static TRecord Load<TRecord>(this DataSource obj, TemporalId id)
            where TRecord : Record
        {
            var result = obj.LoadOrNull<TRecord>(id);
            if (result == null) throw new Exception(
                $"Record with TemporalId={id} is not found in data store {obj.DataSourceName}.");
            return result;
        }

        /// <summary>
        /// Load record from context.DataSource, overriding the dataset
        /// specified in the context with the value specified as the
        /// second parameter. The lookup occurs in the specified dataset
        /// and its imports, expanded to arbitrary depth with repetitions
        /// and cyclic references removed.
        ///
        /// IMPORTANT - this overload of the method loads from loadFrom
        /// dataset, not from context.DataSet.
        ///
        /// If Record property is set, its value is returned without
        /// performing lookup in the data store; otherwise the record
        /// is loaded from storage and cached in Record and the
        /// cached value is returned from subsequent calls.
        ///
        /// Once the record has been cached, the same version will be
        /// returned in subsequent calls with the same key instance.
        /// Create a new key or call earRecord() method to force
        /// reloading new version of the record from storage.
        ///
        /// Error message if the record is not found or is a DeletedRecord.
        /// </summary>
        public static TRecord Load<TKey, TRecord>(this DataSource obj, TypedKey<TKey, TRecord> key, TemporalId loadFrom)
            where TKey : TypedKey<TKey, TRecord>, new()
            where TRecord : TypedRecord<TKey, TRecord>
        {
            // This method will return null if the record is
            // not found or the found record is a DeletedRecord
            var result = obj.LoadOrNull(key, loadFrom);

            // Error message if null, otherwise return
            if (result == null) throw new Exception(
                $"Record with key {key} is not found in dataset with TemporalId={loadFrom}.");

            return result;
        }

        /// <summary>
        /// Save one record to the specified dataset. After the method exits,
        /// record.DataSet will be set to the value of the dataSet parameter.
        ///
        /// All Save methods ignore the value of record.DataSet before the
        /// Save method is called. When dataset is not specified explicitly,
        /// the value of dataset from the context, not from the record, is used.
        /// The reason for this behavior is that the record may be stored from
        /// a different dataset than the one where it is used.
        ///
        /// This method guarantees that TemporalIds of the saved records will be in
        /// strictly increasing order.
        /// </summary>
        public static void SaveOne<TRecord>(this DataSource obj, TRecord record, TemporalId saveTo)
            where TRecord : Record
        {
            // Pass a list of records with one element to SaveMany
            obj.SaveMany(new List<TRecord> { record }, saveTo);
        }

        /// <summary>
        /// Get TemporalId of the dataset with the specified name.
        ///
        /// All of the previously requested dataSetIds are cached by
        /// the data source. To load the latest version of the dataset
        /// written by a separate process, clear the cache first by
        /// calling DataSource.ClearDataSetCache() method.
        ///
        /// Error message if not found.
        /// </summary>
        public static TemporalId GetDataSet(this DataSource obj, string dataSetName)
        {
            // Get dataset or null
            var result = obj.GetDataSetOrNull(dataSetName);

            // Check that it is not null and return
            if (result == null) throw new Exception($"Dataset {dataSetName} is not found in data store {obj.DataSourceName}.");
            return result.Value;
        }

        /// <summary>
        /// Create dataset with the specified dataSetName and no imports.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this DataSource obj, string dataSetName)
        {
            // Create with no imports
            return obj.CreateDataSet(dataSetName, null);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName and imports.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this DataSource obj, string dataSetName, IEnumerable<TemporalId> imports)
        {
            // Make null if enumerable is either null or empty
            var importsList = imports != null ? imports.ToList() : null;
            if (importsList != null && importsList.Count == 0) importsList = null;

            // Create dataset record with the specified name and import
            var result = new DataSet() { DataSetName = dataSetName, Imports = importsList };

            // Save in parentDataSet (this also updates the dictionaries)
            obj.SaveDataSet(result);

            // Return TemporalId that was assigned to the
            // record inside the SaveDataSet method
            return result.Id;
        }
    }
}
