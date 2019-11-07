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
    public interface IDataSource : IDisposable
    {
        //--- PROPERTIES

        /// <summary>
        /// Execution context provides access to key resources including:
        ///
        /// * Logging and error reporting
        /// * Cloud calculation service
        /// * Data sources
        /// * Filesystem
        /// * Progress reporting
        /// </summary>
        IContext Context { get; }

        /// <summary>Unique data source name.</summary>
        string DataSourceName { get; }

        /// <summary>
        /// Flag indicating that the data source is non-temporal.
        ///
        /// For the data stored in data sources where NonTemporal == false,
        /// the data source keeps permanent history of changes to each
        /// record (except where dataset or record are marked as NonTemporal),
        /// and provides the ability to access the record as of the specified
        /// TemporalId, where TemporalId serves as a timeline (records created
        /// later have greater TemporalId than records created earlier).
        ///
        /// For the data stored in data source where NonTemporal == true,
        /// the data source keeps only the latest version of the record. All
        /// datasets created by a NonTemporal data source must also be non-temporal.
        ///
        /// In a non-temporal data source, this flag is ignored as all
        /// datasets in such data source are non-temporal.
        /// </summary>
        bool? NonTemporal { get; }

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
        void Init(IContext context);

        /// <summary>Flush data to permanent storage.</summary>
        void Flush();

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
        TemporalId CreateOrderedTemporalId();

        /// <summary>
        /// Load record by its TemporalId.
        ///
        /// Return null if there is no record for the specified TemporalId;
        /// however an exception will be thrown if the record exists but
        /// is not derived from TRecord.
        /// </summary>
        TRecord LoadOrNull<TRecord>(TemporalId id)
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
        TRecord LoadOrNull<TKey, TRecord>(TypedKey<TKey, TRecord> key, TemporalId loadFrom)
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
        IQuery<TRecord> GetQuery<TRecord>(TemporalId loadFrom)
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
        void SaveMany<TRecord>(IEnumerable<TRecord> records, TemporalId saveTo)
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
        void Delete<TKey, TRecord>(TypedKey<TKey, TRecord> key, TemporalId deleteIn)
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
        void DeleteDb();

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
        TemporalId? GetDataSetOrNull(string dataSetName, TemporalId loadFrom);

        /// <summary>
        /// Save new version of the dataset.
        ///
        /// This method sets Id element of the argument to be the
        /// new TemporalId assigned to the record when it is saved.
        /// The timestamp of the new TemporalId is the current time.
        ///
        /// This method updates in-memory cache to the saved dataset.
        /// </summary>
        void SaveDataSet(DataSet dataSetRecord, TemporalId saveTo);
    }

    /// <summary>Extension methods for IDataSource.</summary>
    public static class IDataSourceExtensions
    {
        /// <summary>
        /// Load record by its TemporalId.
        ///
        /// Error message if there is no record for the specified TemporalId,
        /// or if the record exists but is not derived from TRecord.
        /// </summary>
        public static TRecord Load<TRecord>(this IDataSource obj, TemporalId id)
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
        public static TRecord Load<TKey, TRecord>(this IDataSource obj, TypedKey<TKey, TRecord> key, TemporalId loadFrom)
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
        public static void SaveOne<TRecord>(this IDataSource obj, TRecord record, TemporalId saveTo)
            where TRecord : Record
        {
            // Pass a list of records with one element to SaveMany
            obj.SaveMany(new List<TRecord> { record }, saveTo);
        }

        /// <summary>
        /// Return TemporalId of the latest Common dataset.
        ///
        /// Common dataset is always stored in root dataset.
        /// </summary>
        public static TemporalId GetCommon(this IDataSource obj)
        {
            return obj.GetDataSet(DataSetKey.Common.DataSetName, TemporalId.Empty);
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
        public static TemporalId GetDataSet(this IDataSource obj, string dataSetName, TemporalId loadFrom)
        {
            // Get dataset or null
            var result = obj.GetDataSetOrNull(dataSetName, loadFrom);

            // Check that it is not null and return
            if (result == null) throw new Exception($"Dataset {dataSetName} is not found in data store {obj.DataSourceName}.");
            return result.Value;
        }

        /// <summary>
        /// Create Common dataset with default flags.
        ///
        /// By convention, the Common dataset contains reference and
        /// configuration data and is included as import in all other
        /// datasets.
        ///
        /// The Common dataset is always stored in root dataset.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateCommon(this IDataSource obj)
        {
            // Create with default flags in root dataset
            return obj.CreateCommon(DataSetFlags.Default);
        }

        /// <summary>
        /// Create Common dataset with the specified flags.
        ///
        /// The flags may be used, among other things, to specify
        /// that the created dataset will be NonTemporal even if the
        /// data source is itself temporal. This setting is typically
        /// used to prevent the accumulation of data where history is
        /// not needed.
        ///
        /// By convention, the Common dataset contains reference and
        /// configuration data and is included as import in all other
        /// datasets.
        ///
        /// The Common dataset is always stored in root dataset.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateCommon(this IDataSource obj, DataSetFlags flags)
        {
            // Create with the specified flags in root dataset
            return obj.CreateDataSet("Common", flags, TemporalId.Empty);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName and default flags
        /// in parentDataSet.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IDataSource obj, string dataSetName, TemporalId parentDataSet)
        {
            // If imports are not specified, define with parentDataSet as the only import
            var imports = new TemporalId[] { parentDataSet };

            // Create with default flags in parentDataSet
            return obj.CreateDataSet(dataSetName, imports, DataSetFlags.Default, parentDataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName, specified
        /// imports, and default flags in parentDataSet.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IDataSource obj, string dataSetName, IEnumerable<TemporalId> imports, TemporalId parentDataSet)
        {
            // Create with default flags in parentDataSet
            return obj.CreateDataSet(dataSetName, imports, DataSetFlags.Default, parentDataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName and flags
        /// in context.DataSet, and make context.DataSet its sole import.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IDataSource obj, string dataSetName, DataSetFlags flags, TemporalId parentDataSet)
        {
            // If imports are not specified, define with parent dataset as the only import
            var imports = new TemporalId[] { parentDataSet };

            // Create with the specified flags in parentDataSet
            return obj.CreateDataSet(dataSetName, imports, flags, parentDataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName, imports,
        /// and flags in parentDataSet.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IDataSource obj, string dataSetName, IEnumerable<TemporalId> imports, DataSetFlags flags, TemporalId parentDataSet)
        {
            // Create dataset record with the specified name and import
            var result = new DataSet() { DataSetName = dataSetName, Imports = imports.ToList() };

            // If data source is NonTemporal, dataset will be created
            // as NonTemporal even if not specified by dataset flags
            if (obj.NonTemporal.IsTrue() || (flags & DataSetFlags.NonTemporal) == DataSetFlags.NonTemporal)
            {
                // Make non-temporal if either data source is NonTemporal,
                // or dataset flag for NonTemporal is set
                result.NonTemporal = true;
            }

            // Save in parentDataSet (this also updates the dictionaries)
            obj.SaveDataSet(result, parentDataSet);

            // Return TemporalId that was assigned to the
            // record inside the SaveDataSet method
            return result.Id;
        }
    }
}
