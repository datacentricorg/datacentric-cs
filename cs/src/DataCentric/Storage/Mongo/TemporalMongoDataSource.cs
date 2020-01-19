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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DataCentric
{
    /// <summary>
    /// Temporal data source with datasets based on MongoDB.
    ///
    /// The term Temporal applied means the data source stores complete revision
    /// history including copies of all previous versions of each record.
    ///
    /// In addition to being temporal, this data source is also hierarchical; the
    /// records are looked up across a hierarchy of datasets, including the dataset
    /// itself, its direct Imports, Imports of Imports, etc., ordered by dataset's
    /// TemporalId.
    /// </summary>
    public class TemporalMongoDataSource : MongoDataSource
    {
        /// <summary>Dictionary of collections indexed by type T.</summary>
        private ConcurrentDictionary<Type, object> collectionDict_ = new ConcurrentDictionary<Type, object>();
        private Dictionary<string, TemporalId> dataSetDict_ { get; } = new Dictionary<string, TemporalId>();
        private Dictionary<TemporalId, HashSet<TemporalId>> importDict_ { get; } = new Dictionary<TemporalId, HashSet<TemporalId>>();

        //--- ELEMENTS

        /// <summary>
        /// Records with TemporalId that is greater than or equal to CutoffTime
        /// will be ignored by load methods and queries, and the latest available
        /// record where TemporalId is less than CutoffTime will be returned instead.
        ///
        /// CutoffTime applies to both the records stored in the dataset itself,
        /// and the reports loaded through the Imports list.
        ///
        /// CutoffTime may be set in data source globally, or for a specific dataset
        /// in its details record. If CutoffTime is set for both, the earlier of the
        /// two values will be used.
        /// </summary>
        public TemporalId? CutoffTime { get; set; }

        //--- METHODS

        /// <summary>Flush data to permanent storage.</summary>
        public override void Flush()
        {
            // Do nothing
        }

        /// <summary>
        /// Load record by its TemporalId.
        ///
        /// Return null if there is no record for the specified TemporalId;
        /// however an exception will be thrown if the record exists but
        /// is not derived from TRecord.
        /// </summary>
        public override TRecord LoadOrNull<TRecord>(TemporalId id)
        {
            if (CutoffTime != null)
            {
                // Return null if argument TemporalId is greater than
                // or equal to CutoffTime.
                if (id >= CutoffTime.Value) return null;
            }

            // Find last record in last dataset without constraining record type.
            // The result may not be derived from TRecord.
            var baseResult = GetOrCreateCollection<TRecord>()
                .BaseCollection
                .AsQueryable()
                .FirstOrDefault(p => p.Id == id);

            // Check not only for null but also for the DeletedRecord
            if (baseResult != null && !baseResult.Is<DeletedRecord>())
            {
                // Record is found but we do not yet know if it has the right type.
                // Attempt to cast Record to TRecord and check if the result is null.
                TRecord result = baseResult.As<TRecord>();
                if (result == null)
                {
                    // If cast result is null, the record was found but it is an instance
                    // of class that is not derived from TRecord, in this case the API
                    // requires error message, not returning null
                    throw new Exception(
                        $"Stored type {result.GetType().Name} for TemporalId={id} and " +
                        $"Key={result.Key} is not an instance of the requested type {typeof(TRecord).Name}.");
                }

                // Initialize before returning
                result.Init(Context);
                return result;
            }
            else
            {
                // Record not found or is a DeletedRecord, return null
                return null;
            }
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
        public override TRecord LoadOrNull<TKey, TRecord>(TypedKey<TKey, TRecord> key, TemporalId loadFrom)
        {
            // For pinned records, load from root dataset irrespective of the value of loadFrom
            if (IsPinned<TRecord>()) loadFrom = TemporalId.Empty;

            // String value of the key in semicolon delimited format for use in the query
            string keyValue = key.ToString();

            // Look for exact match of the key
            var baseQueryable = GetOrCreateCollection<TRecord>()
                .BaseCollection
                .AsQueryable()
                .Where(p => p.Key == keyValue);

            // Apply constraints on dataset and revision time
            var queryableWithFinalConstraints = ApplyFinalConstraints(baseQueryable, loadFrom);

            // Order by dataset and then by ID in descending order
            var orderedQueryable = queryableWithFinalConstraints
                .OrderByDescending(p => p.DataSet)
                .OrderByDescending(p => p.Id);

            // Result will be null if the record is not found
            var baseResult = orderedQueryable.FirstOrDefault();

            // Check not only for null but also for the DeletedRecord
            if (baseResult != null && !baseResult.Is<DeletedRecord>())
            {
                // Record is found but we do not yet know if it has the right type.
                // Attempt to cast Record to TRecord and check if the result is null.
                TRecord result = baseResult.As<TRecord>();
                if (result == null)
                {
                    // If cast result is null, the record was found but it is an instance
                    // of class that is not derived from TRecord, in this case the API
                    // requires error message, not returning null
                    throw new Exception(
                        $"Stored type {result.GetType().Name} for Key={key.Value} in " +
                        $"DataSet={loadFrom} is not an instance of the requested type {typeof(TRecord).Name}.");
                }

                // Initialize before returning
                result.Init(Context);
                return result;
            }
            else
            {
                // Record not found or is a DeletedRecord, return null
                return null;
            }
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
        public override IQuery<TRecord> GetQuery<TRecord>(TemporalId loadFrom)
        {
            // For pinned records, load from root dataset irrespective of the value of loadFrom
            if (IsPinned<TRecord>()) loadFrom = TemporalId.Empty;

            // Get or create collection, then create query from collection
            var collection = GetOrCreateCollection<TRecord>();
            return new TemporalMongoQuery<TRecord>(collection, loadFrom);
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
        public override void SaveMany<TRecord>(IEnumerable<TRecord> records, TemporalId saveTo)
        {
            // Error message if data source is readonly or has cutoff time set
            CheckNotReadOnly(saveTo);

            // For pinned records, save to root dataset irrespective of the value of saveTo
            if (IsPinned<TRecord>()) saveTo = TemporalId.Empty;

            // Get collection
            var collection = GetOrCreateCollection<TRecord>();

            // Convert to list unless already a list. The first line
            // will assign null if not already a list, in which case
            // the second line will convert.
            List<TRecord> recordsList = records as List<TRecord>;
            if (recordsList == null) recordsList = records.ToList();

            // Do nothing if the list of records is empty. Checking for zero
            // size here will prevent a subsequent error in InsertMany method
            // of the native Mongo driver.
            if (recordsList.Count == 0) return;

            // Iterate over list elements to populate fields
            foreach (var record in recordsList)
            {
                // This method guarantees that TemporalIds will be in strictly increasing
                // order for this instance of the data source class always, and across
                // all processes and machine if they are not created within the same
                // second.
                var recordId = CreateOrderedTemporalId();

                // TemporalId of the record must be strictly later
                // than TemporalId of the dataset where it is stored
                if (recordId <= saveTo)
                    throw new Exception(
                        $"TemporalId={recordId} of a record must be greater than " +
                        $"TemporalId={saveTo} of the dataset where it is being saved.");

                // Assign ID and DataSet, and only then initialize, because
                // initialization code may use record.ID and record.DataSet
                record.Id = recordId;
                record.DataSet = saveTo;
                record.Init(Context);
            }

            if (IsNonTemporal<TRecord>())
            {
                // Replace the record if exists, or insert if it does not 
                collection.TypedCollection.InsertMany(recordsList); // TODO - replace by Upsert
            }
            else
            {
                // Always insert, previous version will remain in the database
                // but will not be found except through loading by TemporalId,
                // or CutoffTime/ImportsCutoffTime customization
                collection.TypedCollection.InsertMany(recordsList);
            }
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
        public override void Delete<TKey, TRecord>(TypedKey<TKey, TRecord> key, TemporalId deleteIn)
        {
            // Error message if data source is readonly or has CutoffTime set
            CheckNotReadOnly(deleteIn);

            // For pinned records, delete in root dataset irrespective of the value of saveTo
            if (IsPinned<TRecord>()) deleteIn = TemporalId.Empty;

            // Create DeletedRecord with the specified key
            var record = new DeletedRecord {Key = key.Value};

            // Get collection
            var collection = GetOrCreateCollection<TRecord>();

            // This method guarantees that TemporalIds will be in strictly increasing
            // order for this instance of the data source class always, and across
            // all processes and machine if they are not created within the same
            // second.
            var recordId = CreateOrderedTemporalId();
            record.Id = recordId;

            // Assign dataset and then initialize, as the results of
            // initialization may depend on record.DataSet
            record.DataSet = deleteIn;

            // By design, insert will fail if TemporalId is not unique within the collection
            collection.BaseCollection.InsertOne(record);
        }

        /// <summary>
        /// Apply the final constraints after all prior Where clauses but before OrderBy clause:
        ///
        /// * The constraint on dataset lookup list, restricted by CutoffTime (if not null)
        /// * The constraint on ID being strictly less than CutoffTime (if not null)
        /// </summary>
        public IQueryable<TRecord> ApplyFinalConstraints<TRecord>(IQueryable<TRecord> queryable, TemporalId loadFrom)
            where TRecord : Record
        {
            // For pinned records, load from root dataset irrespective of the value of loadFrom
            if (IsPinned<TRecord>()) loadFrom = TemporalId.Empty;

            // Get lookup list by expanding the list of imports to arbitrary
            // depth with duplicates and cyclic references removed.
            //
            // The list will not include datasets that are after the value of
            // CutoffTime if specified, or their imports (including
            // even those imports that are earlier than the constraint).
            IEnumerable<TemporalId> dataSetLookupList = GetDataSetLookupList(loadFrom);

            // Apply constraint that the value is _dataset is
            // one of the elements of dataSetLookupList_
            var result = queryable.Where(p => dataSetLookupList.Contains(p.DataSet));

            // Apply revision time constraint. By making this constraint the
            // last among the constraints, we optimize the use of the index.
            //
            // The property savedBy_ is set using either CutoffTime element.
            // Only one of these two elements can be set at a given time.
            if (CutoffTime != null)
            {
                result = result.Where(p => p.Id < CutoffTime.Value);
            }

            return result;
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
        public override TemporalId? GetDataSetOrNull(string dataSetName)
        {
            if (dataSetDict_.TryGetValue(dataSetName, out TemporalId result))
            {
                // Check if already cached, return if found
                return result;
            }
            else
            {
                // Otherwise load from storage (this also updates the dictionaries)
                DataSetKey dataSetKey = new DataSetKey() { DataSetName = dataSetName };
                DataSet dataSetRecord = this.LoadOrNull(dataSetKey, TemporalId.Empty);

                // If not found, return TemporalId.Empty
                if (dataSetRecord == null) return null;

                // Cache TemporalId for the dataset and its parent
                dataSetDict_[dataSetName] = dataSetRecord.Id;

                // Build and cache dataset lookup list if not found
                if (!importDict_.TryGetValue(dataSetRecord.Id, out HashSet<TemporalId> importSet))
                {
                    importSet = BuildDataSetLookupList(dataSetRecord);
                    importDict_.Add(dataSetRecord.Id, importSet);
                }

                return dataSetRecord.Id;
            }
        }

        /// <summary>
        /// Save new version of the dataset.
        ///
        /// This method sets Id element of the argument to be the
        /// new TemporalId assigned to the record when it is saved.
        /// The timestamp of the new TemporalId is the current time.
        ///
        /// This method updates in-memory cache to the saved dataset.
        /// </summary>
        public override void SaveDataSet(DataSet dataSetRecord)
        {
            // Save dataset to storage. This updates its Id
            // to the new TemporalId created during save
            this.SaveOne<DataSet>(dataSetRecord, TemporalId.Empty);

            // Cache TemporalId for the dataset and its parent
            dataSetDict_[dataSetRecord.Key] = dataSetRecord.Id;

            // Update lookup list dictionary
            var lookupList = BuildDataSetLookupList(dataSetRecord);
            importDict_.Add(dataSetRecord.Id, lookupList);
        }

        /// <summary>
        /// Returns enumeration of import datasets for specified dataset data,
        /// including imports of imports to unlimited depth with cyclic
        /// references and duplicates removed.
        ///
        /// The list will not include datasets that are after the value of
        /// CutoffTime if specified, or their imports (including
        /// even those imports that are earlier than the constraint).
        /// </summary>
        public IEnumerable<TemporalId> GetDataSetLookupList(TemporalId dataSetId)
        {
            // Root dataset has no imports (there is not even a record
            // where these imports can be specified).
            //
            // Return list containing only the root dataset (TemporalId.Empty) and exit
            if (dataSetId == TemporalId.Empty)
            {
                return new TemporalId[] { TemporalId.Empty };
            }

            if (importDict_.TryGetValue(dataSetId, out HashSet<TemporalId> result))
            {
                // Check if the lookup list is already cached, return if yes
                return result;
            }
            else
            {
                // Otherwise load from storage (returns null if not found)
                DataSet dataSetRecord = LoadOrNull<DataSet>(dataSetId);

                if (dataSetRecord == null) throw new Exception($"Dataset with TemporalId={dataSetId} is not found.");
                if (dataSetRecord.DataSet != TemporalId.Empty) throw new Exception($"Dataset with TemporalId={dataSetId} is not stored in root dataset.");

                // Build the lookup list
                result = BuildDataSetLookupList(dataSetRecord);

                // Add to dictionary and return
                importDict_.Add(dataSetId, result);
                return result;
            }
        }

        /// <summary>
        /// Returns true if either data source has NonTemporal flag set,
        /// or record type has NonTemporal attribute.
        /// </summary>
        private bool IsNonTemporal<TRecord>() where TRecord : Record
        {
            // Check NonTemporal attribute for the data source, if set return true.
            if (NonTemporal) return true;

            // Otherwise check NonTemporal attribute for the type, if set return true
            if (typeof(TRecord).GetCustomAttribute<NonTemporalAttribute>(true) != null) return true;

            // Otherwise return false.
            return false;
        }

        /// <summary>
        /// Returns true if the record has Pinned attribute.
        /// </summary>
        private bool IsPinned<TRecord>() where TRecord : Record
        {
            // Check for Pinned attribute for the type, if set return true
            if (typeof(TRecord).GetCustomAttribute<PinnedAttribute>(true) != null) return true;

            // Otherwise return false.
            return false;
        }

        /// <summary>
        /// Gets ImportsCutoffTime from the dataset detail record.
        /// Returns null if dataset detail record is not found.
        /// 
        /// Imported records (records loaded through the Imports list)
        /// where TemporalId is greater than or equal to CutoffTime
        /// will be ignored by load methods and queries, and the latest
        /// available record where TemporalId is less than CutoffTime will
        /// be returned instead.
        ///
        /// This setting only affects records loaded through the Imports
        /// list. It does not affect records stored in the dataset itself.
        ///
        /// Use this feature to freeze Imports as of a given CreatedTime
        /// (part of TemporalId), isolating the dataset from changes to the
        /// data in imported datasets that occur after that time.
        /// </summary>
        public TemporalId? GetImportsCutoffTime(TemporalId dataSetId)
        {
            // TODO - implement when stored in dataset
            return null;
        }

        //--- PRIVATE

        /// <summary>
        /// Returned object holds two collection references - one for the base
        /// type of all records and the other for the record type specified
        /// as generic parameter.
        ///
        /// The need to hold two collection arises from the requirement
        /// that query for a derived type takes into account that another
        /// record with the same key and later dataset or object timestamp
        /// may exist. For this reason, the typed collection is used for
        /// LINQ constraints and base collection is used to iterate over
        /// objects.
        ///
        /// This method also creates indices if they do not exist. The
        /// two default indices are always created:  one for optimizing
        /// loading by key and the other by query.
        ///
        /// Additional indices may be created using class attribute
        /// [IndexElements] for further performance optimization.
        /// </summary>
        private TemporalMongoCollection<TRecord> GetOrCreateCollection<TRecord>()
            where TRecord : Record
        {
            // Check if collection object has already been cached
            // for this type and return cached result if found
            if (collectionDict_.TryGetValue(typeof(TRecord), out object collectionObj))
            {
                var cachedResult = collectionObj.CastTo<TemporalMongoCollection<TRecord>>();
                return cachedResult;
            }

            // Check that scalar discriminator convention is set for TRecord
            var discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(TRecord));
            if (useScalarDiscriminatorConvention_)
            {
                if (!discriminatorConvention.Is<ScalarDiscriminatorConvention>())
                    throw new Exception(
                        $"Scalar discriminator convention is not set for type {typeof(TRecord).Name}. " +
                        $"The convention should have been set set in the static constructor of " +
                        $"MongoDataSource");
            }
            else
            {
                if (!discriminatorConvention.Is<HierarchicalDiscriminatorConvention>())
                    throw new Exception(
                        $"Hierarchical discriminator convention is not set for type {typeof(TRecord).Name}. " +
                        $"The convention should have been set set in the static constructor of " +
                        $"MongoDataSource");
            }

            // Collection name is root class name of the record without prefix
            string collectionName = DataTypeInfo.GetOrCreate<TRecord>().GetCollectionName();

            // Get interfaces to base and typed collections for the same name
            var baseCollection = Db.GetCollection<Record>(collectionName);
            var typedCollection = Db.GetCollection<TRecord>(collectionName);

            //--- Load standard index types

            // Each data type has an index for optimized loading by key.
            // This index consists of Key in ascending order, followed by
            // DataSet and ID in descending order.
            var loadIndexKeys = Builders<TRecord>.IndexKeys
                .Ascending(new StringFieldDefinition<TRecord>("_key")) // .Key
                .Descending(new StringFieldDefinition<TRecord>("_dataset")) // .DataSet
                .Descending(new StringFieldDefinition<TRecord>("_id")); // .Id

            // Use index definition convention to specify the index name
            var loadIndexName = "Key-DataSet-Id";
            var loadIndexModel = new CreateIndexModel<TRecord>(loadIndexKeys, new CreateIndexOptions { Name = loadIndexName });
            typedCollection.Indexes.CreateOne(loadIndexModel);

            //--- Load custom index types

            // Additional indices are provided using IndexAttribute for the class.
            // Get a sorted dictionary of (definition, name) pairs
            // for the inheritance chain of the specified type.
            var indexDict = IndexElementsAttribute.GetAttributesDict<TRecord>();

            // Iterate over the dictionary to define the index
            foreach (var indexInfo in indexDict)
            {
                string indexDefinition = indexInfo.Key;
                string indexName = indexInfo.Value;

                // Parse index definition to get a list of (ElementName,SortOrder) tuples
                List<(string, int)> indexTokens = IndexElementsAttribute.ParseDefinition<TRecord>(indexDefinition);

                var indexKeysBuilder = Builders<TRecord>.IndexKeys;
                IndexKeysDefinition<TRecord> indexKeys = null;

                // Iterate over (ElementName,SortOrder) tuples
                foreach (var indexToken in indexTokens)
                {
                    (string elementName, int sortOrder) = indexToken;

                    if (indexKeys == null)
                    {
                        // Create from builder for the first element
                        if (sortOrder == 1) indexKeys = indexKeysBuilder.Ascending(new StringFieldDefinition<TRecord>(elementName));
                        else if (sortOrder == -1) indexKeys = indexKeysBuilder.Descending(new StringFieldDefinition<TRecord>(elementName));
                        else throw new Exception("Sort order must be 1 or -1.");
                    }
                    else
                    {
                        // Chain to the previous list of index keys for the remaining elements
                        if (sortOrder == 1) indexKeys = indexKeys.Ascending(new StringFieldDefinition<TRecord>(elementName));
                        else if (sortOrder == -1) indexKeys = indexKeys.Descending(new StringFieldDefinition<TRecord>(elementName));
                        else throw new Exception("Sort order must be 1 or -1.");
                    }
                }

                if (indexName == null) throw new Exception("Index name cannot be null.");
                var indexModel = new CreateIndexModel<TRecord>(indexKeys, new CreateIndexOptions { Name = indexName });

                // Add to indexes for the collection
                typedCollection.Indexes.CreateOne(indexModel);
            }

            // Create result that holds both base and typed collections
            TemporalMongoCollection<TRecord> result = new TemporalMongoCollection<TRecord>(this, baseCollection, typedCollection);

            // Add the result to the collection dictionary and return
            collectionDict_.TryAdd(typeof(TRecord), result);
            return result;
        }

        /// <summary>
        /// Builds hashset of import datasets for specified dataset data,
        /// including imports of imports to unlimited depth with cyclic
        /// references and duplicates removed. This method uses cached lookup
        /// list for the import datasets but not for the argument dataset.
        ///
        /// The list will not include datasets that are after the value of
        /// CutoffTime if specified, or their imports (including
        /// even those imports that are earlier than the constraint).
        ///
        /// This overload of the method will return the result hashset.
        ///
        /// This private helper method should not be used directly.
        /// It provides functionality for the public API of this class.
        /// </summary>
        private HashSet<TemporalId> BuildDataSetLookupList(DataSet dataSetRecord)
        {
            // Delegate to the second overload
            var result = new HashSet<TemporalId>();
            BuildDataSetLookupList(dataSetRecord, result);
            return result;
        }

        /// <summary>
        /// Builds hashset of import datasets for specified dataset data,
        /// including imports of imports to unlimited depth with cyclic
        /// references and duplicates removed. This method uses cached lookup
        /// list for the import datasets but not for the argument dataset.
        ///
        /// The list will not include datasets that are after the value of
        /// CutoffTime if specified, or their imports (including
        /// even those imports that are earlier than the constraint).
        ///
        /// This overload of the method will return the result hashset.
        ///
        /// This private helper method should not be used directly.
        /// It provides functionality for the public API of this class.
        /// </summary>
        private void BuildDataSetLookupList(DataSet dataSetRecord, HashSet<TemporalId> result)
        {
            // Return if the dataset is null or has no imports
            if (dataSetRecord == null) return;

            // Error message if dataset has no Id or Key set
            dataSetRecord.Id.CheckHasValue();
            dataSetRecord.Key.CheckHasValue();

            if (CutoffTime != null && dataSetRecord.Id >= CutoffTime.Value)
            {
                // Do not add if revision time constraint is set and is before this dataset.
                // In this case the import datasets should not be added either, even if they
                // do not fail the revision time constraint
                return;
            }

            // Add self to the result
            result.Add(dataSetRecord.Id);

            // Add imports to the result
            if (dataSetRecord.Imports != null)
            {
                foreach (var dataSetId in dataSetRecord.Imports)
                {
                    // Dataset cannot include itself as its import
                    if (dataSetRecord.Id == dataSetId)
                        throw new Exception(
                            $"Dataset {dataSetRecord.Key} with TemporalId={dataSetRecord.Id} includes itself in the list of its imports.");

                    // The Add method returns true if the argument is not yet present in the hashset
                    if (result.Add(dataSetId))
                    {
                        // Add recursively if not already present in the hashset
                        var cachedImportList = GetDataSetLookupList(dataSetId);
                        foreach (var importId in cachedImportList)
                        {
                            result.Add(importId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Error message if either ReadOnly flag or CutoffTime is set
        /// for the data source.
        /// </summary>
        private void CheckNotReadOnly(TemporalId dataSetId)
        {
            if (ReadOnly)
                throw new Exception(
                    $"Attempting write operation for data source {DataSourceName} where ReadOnly flag is set.");

            if (CutoffTime != null)
                throw new Exception(
                    $"Attempting write operation for data source {DataSourceName} where " +
                    $"CutoffTime is set. Historical view of the data cannot be written to.");
        }
    }
}
