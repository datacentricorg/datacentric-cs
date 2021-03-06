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
using System.Reflection;

namespace DataCentric
{
    /// <summary>
    /// Context interface provides:
    ///
    /// * Default data source
    /// * Default dataset of the default data source
    /// * Logging
    /// * Progress reporting
    /// * Filesystem access (if available)
    /// 
    /// The context is configured by setting each of the service
    /// properties (e.g. Log, Progress, etc.). The setter then
    /// invokes Init(...) method of the property and passes self
    /// as argument. Detailed error message is provided when a
    /// property is accessed before it is set.
    ///
    /// This class is a non-abstract base. It can be used
    /// directly or as base class of other context implementations.
    /// </summary>
    public class Context : IDisposable
    {
        private IFolder outputFolder_;
        private Log log_;
        private Progress progress_;
        private DataSource dataSource_;
        private TemporalId? dataSet_;

        /// <summary>
        /// Provides a unified API for an output folder located in a
        /// conventional filesystem or an alternative backing store
        /// such as S3.
        /// </summary>
        public IFolder OutputFolder
        {
            get
            {
                if (outputFolder_ == null) throw new Exception($"OutputFolder property is not set in {GetType().Name}.");
                return outputFolder_;
            }
            set
            {
                outputFolder_ = value;
                outputFolder_.Init(this);
            }
        }

        /// <summary>Logging interface.</summary>
        public Log Log
        {
            get
            {
                if (log_ == null) throw new Exception($"Log property is not set in {GetType().Name}.");
                return log_;
            }
            set
            {
                log_ = value;
                log_.Init(this);
            }
        }

        /// <summary>Progress interface.</summary>
        public Progress Progress
        {
            get
            {
                if (progress_ == null) throw new Exception($"Progress property is not set in {GetType().Name}.");
                return progress_;
            }
            set
            {
                progress_ = value;
                progress_.Init(this);
            }
        }

        /// <summary>Default data source of the context.</summary>
        public DataSource DataSource
        {
            get
            {
                if (dataSource_ == null) throw new Exception($"DataSource property is not set in {GetType().Name}.");
                return dataSource_;
            }
            set
            {
                dataSource_ = value;
                dataSource_.Init(this);
            }
        }

        /// <summary>Default dataset of the context.</summary>
        public TemporalId DataSet
        {
            get
            {
                if (dataSet_ == null) throw new Exception($"DataSet property is not set in {GetType().Name}.");
                return dataSet_.Value;
            }
            set
            {
                dataSet_ = value;
            }
        }

        /// <summary>
        /// Set this property to true to keep test data after the
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
        public bool KeepTestData { get; set; }

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
            // Call Dispose() for each initialized property of the context
            // in the reverse order of initialization
            if (outputFolder_ != null) outputFolder_.Dispose();
            if (log_ != null) log_.Dispose();
            if (progress_ != null) progress_.Dispose();
            if (dataSource_ != null) dataSource_.Dispose();

            // Uncomment except in root class of the hierarchy
            // base.Dispose();
        }

        /// <summary>Flush data to permanent storage.</summary>
        public virtual void Flush()
        {
            // Uncomment except in root class of the hierarchy
            // base.Flush();

            // Call Flush() for each initialized property of the context
            if (outputFolder_ != null) outputFolder_.Flush();
            if (log_ != null) log_.Flush();
            if (progress_ != null) progress_.Flush();
            if (dataSource_ != null) dataSource_.Flush();
        }
    }

    /// <summary>
    /// Extension methods for Context.
    ///
    /// This class permits the methods of DataSource to be called for
    /// Context by forwarding the implementation to Context.DataSource.
    /// </summary>
    public static class ContextExtensions
    {
        /// <summary>
        /// Load record by its TemporalId.
        ///
        /// Error message if there is no record for the specified TemporalId,
        /// or if the record exists but is not derived from TRecord.
        /// </summary>
        public static TRecord Load<TRecord>(this Context obj, TemporalId id)
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
        public static TRecord LoadOrNull<TRecord>(this Context obj, TemporalId id)
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
        public static TRecord Load<TKey, TRecord>(this Context obj, TypedKey<TKey, TRecord> key)
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
        public static TRecord Load<TKey, TRecord>(this Context obj, TypedKey<TKey, TRecord> key, TemporalId loadFrom)
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
        public static TRecord LoadOrNull<TKey, TRecord>(this Context obj, TypedKey<TKey, TRecord> key, TemporalId loadFrom)
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
        public static IQuery<TRecord> GetQuery<TRecord>(this Context obj)
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
        public static IQuery<TRecord> GetQuery<TRecord>(this Context obj, TemporalId loadFrom)
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
        public static void SaveOne<TRecord>(this Context obj, TRecord record)
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
        public static void SaveOne<TRecord>(this Context obj, TRecord record, TemporalId saveTo)
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
        public static void SaveMany<TRecord>(this Context obj, IEnumerable<TRecord> records)
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
        public static void SaveMany<TRecord>(this Context obj, IEnumerable<TRecord> records, TemporalId saveTo)
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
        public static void Delete<TKey, TRecord>(this Context obj, TypedKey<TKey, TRecord> key)
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
        public static void Delete<TKey, TRecord>(this Context obj, TypedKey<TKey, TRecord> key, TemporalId deleteIn)
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
        public static void DeleteDb(this Context obj)
        {
            obj.DataSource.DeleteDb();
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
        public static TemporalId GetDataSet(this Context obj, string dataSetName)
        {
            return obj.DataSource.GetDataSet(dataSetName);
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
        public static TemporalId? GetDataSetOrNull(this Context obj, string dataSetName)
        {
            return obj.DataSource.GetDataSetOrNull(dataSetName);
        }

        /// <summary>
        /// Create dataset with the specified dataSetName and no imports.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this Context obj, string dataSetName)
        {
            return obj.DataSource.CreateDataSet(dataSetName);
        }

        /// <summary>
        /// Create dataset with the specified dataSetNam and imports.
        ///
        /// This method updates in-memory dataset cache to include
        /// the created dataset.
        /// </summary>
        public static TemporalId CreateDataSet(this Context obj, string dataSetName, IEnumerable<TemporalId> imports)
        {
            return obj.DataSource.CreateDataSet(dataSetName, imports);
        }

        /// <summary>
        /// Save the specified dataset record in context.DataSet.
        ///
        /// This method updates in-memory dataset cache to include
        /// the saved dataset.
        /// </summary>
        public static void SaveDataSet(this Context obj, DataSet dataSetRecord)
        {
            obj.DataSource.SaveDataSet(dataSetRecord);
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
        public static void Configure(this Context obj)
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

                        // Ensure the method has Context as its only parameter
                        var paramsInfo = configureMethod.GetParameters();
                        if (paramsInfo.Length != 1) throw new Exception(
                            $"Type {type.Name} is marked by [Configurable] attribute " +
                            $"and implements Configure(...) method, but this method has " +
                            $"{paramsInfo.Length} parameters while it should have one parameter " +
                            $"with type Context.");
                        if (paramsInfo[0].ParameterType != typeof(Context)) throw new Exception(
                            $"Type {type.Name} is marked by [Configurable] attribute " +
                            $"and implements Configure(...) method with one parameter, but the" +
                            $"type of this parameter is not Context.");

                        // Invoke with the current context as its only parameter
                        configureMethod.Invoke(null, new object[] { obj });
                    }
                }
            }
        }
    }
}
