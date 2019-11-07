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
using System.ComponentModel;
using System.Reflection;

namespace DataCentric
{
    /// <summary>
    /// Context defines dataset and provides access to data,
    /// logging, and other supporting functionality.
    /// </summary>
    public interface IContext : IDisposable
    {
        /// <summary>
        /// Provides a unified API for an output folder located in a
        /// conventional filesystem or an alternative backing store
        /// such as S3.
        /// </summary>
        IFolder OutputFolder { get; }

        /// <summary>Logging interface.</summary>
        ILog Log { get; }

        /// <summary>Progress interface.</summary>
        IProgress Progress { get; }

        /// <summary>Default data source of the context.</summary>
        IDataSource DataSource { get; }

        /// <summary>Default dataset of the context.</summary>
        TemporalId DataSet { get; }

        //--- METHODS

        /// <summary>Flush data to permanent storage.</summary>
        void Flush();

        /// <summary>
        /// Invoke this method to keep test data after the
        /// test method exits.
        ///
        /// When running under xUnit, the data in test database is not
        /// erased on test method exit if KeepTestData() was invoked.
        ///
        /// When running under DataCentric, the test dataset will not
        /// be deleted on test method exit if KeepTestData() was invoked.
        ///
        /// Note that test data is always erased when test method enters,
        /// irrespective of any KeepTestData() calls and irrespective of
        /// whether or not KeepTestData() has been called.
        /// </summary>
        void KeepTestData();
    }

    /// <summary>
    /// Extension methods for IContext.
    ///
    /// This class permits the methods of IDataSource to be called for
    /// IContext by forwarding the implementation to IContext.DataSource.
    /// </summary>
    public static class IContextExtensions
    {
        /// <summary>
        /// Load record by its TemporalId.
        ///
        /// Error message if there is no record for the specified TemporalId,
        /// or if the record exists but is not derived from TRecord.
        /// </summary>
        public static TRecord Load<TRecord>(this IContext obj, TemporalId id)
            where TRecord : Record
        {
            return obj.DataSource.Load<TRecord>(id);
        }

        /// <summary>
        /// Load record by its TemporalId.
        ///
        /// Return null if there is no record for the specified TemporalId;
        /// however an exception will be thrown if the record exists but
        /// is not derived from TRecord.
        /// </summary>
        public static TRecord LoadOrNull<TRecord>(this IContext obj, TemporalId id)
            where TRecord : Record
        {
            return obj.DataSource.LoadOrNull<TRecord>(id);
        }

        /// <summary>
        /// Load record from context.DataSource, overriding the dataset
        /// specified in the context with the value specified as the
        /// second parameter. The lookup occurs in the specified dataset
        /// and its imports, expanded to arbitrary depth with repetitions
        /// and cyclic references removed.
        ///
        /// This overload of the method loads from from context.DataSet.
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
        public static TRecord Load<TKey, TRecord>(this IContext obj, TypedKey<TKey, TRecord> key)
            where TKey : TypedKey<TKey, TRecord>, new()
            where TRecord : TypedRecord<TKey, TRecord>
        {
            return obj.DataSource.Load(key, obj.DataSet);
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
        public static TRecord Load<TKey, TRecord>(this IContext obj, TypedKey<TKey, TRecord> key, TemporalId loadFrom)
            where TKey : TypedKey<TKey, TRecord>, new()
            where TRecord : TypedRecord<TKey, TRecord>
        {
            return obj.DataSource.Load(key, loadFrom);
        }

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
        public static TRecord LoadOrNull<TKey, TRecord>(this IContext obj, TypedKey<TKey, TRecord> key, TemporalId loadFrom)
            where TKey : TypedKey<TKey, TRecord>, new()
            where TRecord : TypedRecord<TKey, TRecord>
        {
            return obj.DataSource.LoadOrNull(key, loadFrom);
        }

        /// <summary>
        /// Get query for the specified type in the dataset of the context.
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
        public static IQuery<TRecord> GetQuery<TRecord>(this IContext obj)
            where TRecord : Record
        {
            return obj.DataSource.GetQuery<TRecord>(obj.DataSet);
        }

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
        public static IQuery<TRecord> GetQuery<TRecord>(this IContext obj, TemporalId loadFrom)
            where TRecord : Record
        {
            return obj.DataSource.GetQuery<TRecord>(loadFrom);
        }

        /// <summary>
        /// Save record to the specified dataset. After the method exits,
        /// record.DataSet will be set to the value of the dataSet parameter.
        ///
        /// All Save methods ignore the value of record.DataSet before the
        /// Save method is called. When dataset is not specified explicitly,
        /// the value of dataset from the context, not from the record, is used.
        /// The reason for this behavior is that the record may be stored from
        /// a different dataset than the one where it is used.
        ///
        /// This method guarantees that TemporalIds will be in strictly increasing
        /// order for this instance of the data source class always, and across
        /// all processes and machine if they are not created within the same
        /// second.
        /// </summary>
        public static void SaveOne<TRecord>(this IContext obj, TRecord record)
            where TRecord : Record
        {
            // All Save methods ignore the value of record.DataSet before the
            // Save method is called. When dataset is not specified explicitly,
            // the value of dataset from the context, not from the record, is used.
            // The reason for this behavior is that the record may be stored from
            // a different dataset than the one where it is used.
            obj.DataSource.SaveOne(record, obj.DataSet);
        }

        /// <summary>
        /// Save record to the specified dataset. After the method exits,
        /// record.DataSet will be set to the value of the dataSet parameter.
        ///
        /// All Save methods ignore the value of record.DataSet before the
        /// Save method is called. When dataset is not specified explicitly,
        /// the value of dataset from the context, not from the record, is used.
        /// The reason for this behavior is that the record may be stored from
        /// a different dataset than the one where it is used.
        ///
        /// This method guarantees that TemporalIds will be in strictly increasing
        /// order for this instance of the data source class always, and across
        /// all processes and machine if they are not created within the same
        /// second.
        /// </summary>
        public static void SaveOne<TRecord>(this IContext obj, TRecord record, TemporalId saveTo)
            where TRecord : Record
        {
            obj.DataSource.SaveOne(record, saveTo);
        }

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
        public static void SaveMany<TRecord>(this IContext obj, IEnumerable<TRecord> records)
            where TRecord : Record
        {
            // All Save methods ignore the value of record.DataSet before the
            // Save method is called. When dataset is not specified explicitly,
            // the value of dataset from the context, not from the record, is used.
            // The reason for this behavior is that the record may be stored from
            // a different dataset than the one where it is used.
            obj.DataSource.SaveMany(records, obj.DataSet);
        }

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
        public static void SaveMany<TRecord>(this IContext obj, IEnumerable<TRecord> records, TemporalId saveTo)
            where TRecord : Record
        {
            obj.DataSource.SaveMany(records, saveTo);
        }

        /// <summary>
        /// Write a DeletedRecord for the dataset of the context and the specified
        /// key instead of actually deleting the record. This ensures that
        /// a record in another dataset does not become visible during
        /// lookup in a sequence of datasets.
        ///
        /// To avoid an additional roundtrip to the data store, the delete
        /// marker is written even when the record does not exist.
        /// </summary>
        public static void Delete<TKey, TRecord>(this IContext obj, TypedKey<TKey, TRecord> key)
            where TKey : TypedKey<TKey, TRecord>, new()
            where TRecord : TypedRecord<TKey, TRecord>
        {
            // Delete in the dataset of the context
            obj.DataSource.Delete(key, obj.DataSet);
        }

        /// <summary>
        /// Write a DeletedRecord in deleteIn dataset for the specified key
        /// instead of actually deleting the record. This ensures that
        /// a record in another dataset does not become visible during
        /// lookup in a sequence of datasets.
        ///
        /// To avoid an additional roundtrip to the data store, the delete
        /// marker is written even when the record does not exist.
        /// </summary>
        public static void Delete<TKey, TRecord>(this IContext obj, TypedKey<TKey, TRecord> key, TemporalId deleteIn)
            where TKey : TypedKey<TKey, TRecord>, new()
            where TRecord : TypedRecord<TKey, TRecord>
        {
            obj.DataSource.Delete(key, deleteIn);
        }

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
        public static void DeleteDb(this IContext obj)
        {
            obj.DataSource.DeleteDb();
        }

        /// <summary>
        /// Return TemporalId of the latest Common dataset.
        ///
        /// Common dataset is always stored in root dataset.
        /// </summary>
        public static TemporalId GetCommon(this IContext obj)
        {
            return obj.DataSource.GetCommon();
        }

        /// <summary>
        /// Get TemporalId of the dataset with the specified name.
        ///
        /// This overload of the GetDataSet method does not
        /// specify the loadFrom parameter explicitly and instead
        /// uses context.DataSet for its value.
        ///
        /// All of the previously requested dataSetIds are cached by
        /// the data source. To load the latest version of the dataset
        /// written by a separate process, clear the cache first by
        /// calling DataSource.ClearDataSetCache() method.
        ///
        /// Error message if not found.
        /// </summary>
        public static TemporalId GetDataSet(this IContext obj, string dataSetName)
        {
            return obj.DataSource.GetDataSet(dataSetName, obj.DataSet);
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
        public static TemporalId GetDataSet(this IContext obj, string dataSetName, TemporalId loadFrom)
        {
            return obj.DataSource.GetDataSet(dataSetName, loadFrom);
        }

        /// <summary>
        /// Get TemporalId of the dataset with the specified name.
        ///
        /// This overload of the GetDataSetOrNull method does not
        /// specify the loadFrom parameter explicitly and instead
        /// uses context.DataSet for its value.
        ///
        /// All of the previously requested dataSetIds are cached by
        /// the data source. To load the latest version of the dataset
        /// written by a separate process, clear the cache first by
        /// calling DataSource.ClearDataSetCache() method.
        ///
        /// Returns null if not found.
        /// </summary>
        public static TemporalId? GetDataSetOrNull(this IContext obj, string dataSetName)
        {
            return obj.DataSource.GetDataSetOrNull(dataSetName, obj.DataSet);
        }

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
        public static TemporalId? GetDataSetOrNull(this IContext obj, string dataSetName, TemporalId loadFrom)
        {
            return obj.DataSource.GetDataSetOrNull(dataSetName, loadFrom);
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
        public static TemporalId CreateCommon(this IContext obj)
        {
            return obj.DataSource.CreateCommon();
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
        public static TemporalId CreateCommon(this IContext obj, DataSetFlags flags)
        {
            return obj.DataSource.CreateCommon(flags);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName and default flags
        /// in context.DataSet, and make context.DataSet its sole import.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IContext obj, string dataSetName)
        {
            return obj.DataSource.CreateDataSet(dataSetName, obj.DataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName and default flags
        /// in parentDataSet, and make context.DataSet its sole import.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IContext obj, string dataSetName, TemporalId parentDataSet)
        {
            return obj.DataSource.CreateDataSet(dataSetName, parentDataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName, specified imports,
        /// and default flags in context.DataSet.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IContext obj, string dataSetName, IEnumerable<TemporalId> imports)
        {
            return obj.DataSource.CreateDataSet(dataSetName, imports, obj.DataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName, specified
        /// imports, and default flags in parentDataSet.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IContext obj, string dataSetName, IEnumerable<TemporalId> imports, TemporalId parentDataSet)
        {
            return obj.DataSource.CreateDataSet(dataSetName, imports, parentDataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName and flags
        /// in context.DataSet, and make context.DataSet its sole import.
        ///
        /// The flags may be used, among other things, to specify
        /// that the created dataset will be NonTemporal even if the
        /// data source is itself temporal. This setting is typically
        /// used to prevent the accumulation of data where history is
        /// not needed.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IContext obj, string dataSetName, DataSetFlags flags)
        {
            return obj.DataSource.CreateDataSet(dataSetName, flags, obj.DataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName and flags
        /// in parentDataSet, and make parentDataSet its sole import.
        ///
        /// The flags may be used, among other things, to specify
        /// that the created dataset will be NonTemporal even if the
        /// data source is itself temporal. This setting is typically
        /// used to prevent the accumulation of data where history is
        /// not needed.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IContext obj, string dataSetName, DataSetFlags flags, TemporalId parentDataSet)
        {
            return obj.DataSource.CreateDataSet(dataSetName, flags, parentDataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName, imports,
        /// and flags in context.DataSet.
        ///
        /// The flags may be used, among other things, to specify
        /// that the created dataset will be NonTemporal even if the
        /// data source is itself temporal. This setting is typically
        /// used to prevent the accumulation of data where history is
        /// not needed.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IContext obj, string dataSetName, IEnumerable<TemporalId> imports, DataSetFlags flags)
        {
            return obj.DataSource.CreateDataSet(dataSetName, imports, flags, obj.DataSet);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName, imports,
        /// and flags in parentDataSet.
        ///
        /// The flags may be used, among other things, to specify
        /// that the created dataset will be NonTemporal even if the
        /// data source is itself temporal. This setting is typically
        /// used to prevent the accumulation of data where history is
        /// not needed.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this IContext obj, string dataSetName, IEnumerable<TemporalId> imports, DataSetFlags flags, TemporalId parentDataSet)
        {
            return obj.DataSource.CreateDataSet(dataSetName, imports, flags, parentDataSet);
        }

        /// <summary>
        /// Save the specified dataset record in context.DataSet.
        ///
        /// This method updates in-memory dataset cache to include
        /// the saved dataset.
        /// </summary>
        public static void SaveDataSet(this IContext obj, DataSet dataSetRecord)
        {
            obj.DataSource.SaveDataSet(dataSetRecord, obj.DataSet);
        }

        /// <summary>
        /// Save the specified dataset record in parentDataSet.
        ///
        /// This method updates in-memory dataset cache to include
        /// the saved dataset.
        /// </summary>
        public static void SaveDataSet(this IContext obj, DataSet dataSetRecord, TemporalId parentDataSet)
        {
            obj.DataSource.SaveDataSet(dataSetRecord, parentDataSet);
        }

        /// <summary>
        /// Invoke static Configure(context) method with self as argument for
        /// every class that is accessible by the executing assembly and marked
        /// with [Configurable] attribute.
        ///
        /// The method Configure(context) may be used to configure:
        ///
        /// * Reference data, and
        /// * In case of test mocks, test data
        ///
        /// The order in which Configure(context) method is invoked when
        /// multiple classes marked by [Configurable] attribute are present
        /// is undefined. The implementation of Configure(context) should
        /// not rely on any existing data, and should not invoke other
        /// Configure(context) method of other classes.
        ///
        /// The attribute [Configurable] is not inherited. To invoke
        /// Configure(context) method for multiple classes within the same
        /// inheritance chain, specify [Configurable] attribute for each
        /// class that provides Configure(context) method.
        /// </summary>
        public static void Configure(this IContext obj)
        {
            // Iterate over types that in every assembly loaded into the domain
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    // Iterate over classes that implement [Configurable] attribute directly,
                    // not through inheritance
                    var filteredAttributes = type.GetCustomAttributes(typeof(ConfigurableAttribute), false);
                    if (filteredAttributes.Length > 0)
                    {
                        // Ensure the method is present
                        var configureMethod = type.GetMethod("Configure");
                        if (configureMethod == null)
                            throw new Exception(
                                $"Type {type.Name} is marked by [Configurable] attribute " +
                                $"but does not implement Configure(context) method.");

                        // Ensure the method is static and public
                        if (!configureMethod.IsStatic) throw new Exception(
                            $"Type {type.Name} is marked by [Configurable] attribute " +
                            $"and implements Configure(...) method, but this method is " +
                            $"not static.");
                        if (!configureMethod.IsPublic) throw new Exception(
                            $"Type {type.Name} is marked by [Configurable] attribute " +
                            $"and implements Configure(...) method, but this method is " +
                            $"not public.");

                        // Ensure the method has IContext as its only parameter
                        var paramsInfo = configureMethod.GetParameters();
                        if (paramsInfo.Length != 1) throw new Exception(
                            $"Type {type.Name} is marked by [Configurable] attribute " +
                            $"and implements Configure(...) method, but this method has " +
                            $"{paramsInfo.Length} parameters while it should have one parameter " +
                            $"with type IContext.");
                        if (paramsInfo[0].ParameterType != typeof(IContext)) throw new Exception(
                            $"Type {type.Name} is marked by [Configurable] attribute " +
                            $"and implements Configure(...) method with one parameter, but the" +
                            $"type of this parameter is not IContext.");

                        // Invoke with the current context as its only parameter
                        configureMethod.Invoke(null, new object[] { obj });
                    }
                }
            }
        }
    }
}
