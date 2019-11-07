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
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DataCentric
{
    /// <summary>
    /// Implements IQuery for temporal MongoDB data source.
    ///
    /// This implementation combines methods of IQueryable(TRecord) with
    /// additional constraints and ordering to retrieve the correct version
    /// of the record across multiple datasets.
    /// </summary>
    public class TemporalMongoQuery<TRecord> : IQuery<TRecord>
        where TRecord : Record
    {
        private readonly TemporalMongoCollection<TRecord> collection_;
        private readonly TemporalId loadFrom_;
        private readonly IQueryable<TRecord> queryable_;
        private readonly IOrderedQueryable<TRecord> orderedQueryable_;

        //--- CONSTRUCTORS

        /// <summary>
        /// Create query from collection and dataset.
        /// </summary>
        public TemporalMongoQuery(TemporalMongoCollection<TRecord> collection, TemporalId loadFrom)
        {
            collection_ = collection;
            loadFrom_ = loadFrom;

            // Create queryable from typed collection rather than base
            // collection so LINQ queries can be applied to properties
            // of the generic parameter type TRecord
            var queryable = collection_.TypedCollection.AsQueryable();

            // Without explicitly applying OfType, the match for _t
            // may or may not be added. This step ensures the match
            // for _t exists and is the first stage in the aggregation
            // pipeline
            queryable_ = queryable.OfType<TRecord>();
        }

        /// <summary>
        /// Create from collection for the base type Record, the lookup
        /// list of datasets, and IQueryable(TRecord).
        ///
        /// This constructor is private and is intended for use by the
        /// implementation of this class only.
        /// </summary>
        private TemporalMongoQuery(TemporalMongoCollection<TRecord> collection, TemporalId loadFrom, IQueryable<TRecord> queryable)
        {
            if (queryable == null)
                throw new Exception(
                    "Attempting to create a query from a null queryable.");

            collection_ = collection;
            loadFrom_ = loadFrom;
            queryable_ = queryable;
        }

        /// <summary>
        /// Create from collection for the base type Record, the lookup
        /// list of datasets, and IOrderedQueryable(TRecord).
        ///
        /// This constructor is private and is intended for use by the
        /// implementation of this class only.
        /// </summary>
        private TemporalMongoQuery(TemporalMongoCollection<TRecord> collection, TemporalId loadFrom, IOrderedQueryable<TRecord> orderedQueryable)
        {
            if (orderedQueryable == null)
                throw new Exception(
                    "Attempting to create a query from a null orderedQueryable.");

            collection_ = collection;
            loadFrom_ = loadFrom;
            orderedQueryable_ = orderedQueryable;
        }

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
        public IContext Context { get => collection_.DataSource.Context; }

        //--- METHODS

        /// <summary>Filters a sequence of values based on a predicate.</summary>
        public IQuery<TRecord> Where(Expression<Func<TRecord, bool>> predicate)
        {
            if (queryable_ != null && orderedQueryable_ == null)
            {
                return new TemporalMongoQuery<TRecord>(collection_, loadFrom_, queryable_.Where(predicate));
            }
            else if (queryable_ == null && orderedQueryable_ != null)
            {
                throw new Exception(
                    "All Where(...) clauses of the query must precede " +
                    "SortBy(...) or SortByDescending(...) clauses of the same query.");
            }
            else
            {
                throw new Exception(
                    "Strictly one of queryable_ or orderedQueryable_ can " +
                    "have value, not both and not neither.");
            }
        }

        /// <summary>Sorts the elements of a sequence in ascending order according to the selected key.</summary>
        public IQuery<TRecord> SortBy<TProperty>(Expression<Func<TRecord, TProperty>> keySelector)
        {
            if (queryable_ != null && orderedQueryable_ == null)
            {
                // Apply dataset constraint before ordering
                var queryableWithDataSetConstraint = collection_.DataSource.ApplyFinalConstraints(queryable_, loadFrom_);

                // First SortBy clause, use OrderBy of queryable_
                return new TemporalMongoQuery<TRecord>(collection_, loadFrom_, queryableWithDataSetConstraint.OrderBy(keySelector));
            }
            else if (queryable_ == null && orderedQueryable_ != null)
            {
                // Subsequent SortBy clauses, use ThenBy of the orderedQueryable_
                return new TemporalMongoQuery<TRecord>(collection_, loadFrom_, orderedQueryable_.ThenBy(keySelector));
            }
            else
            {
                throw new Exception(
                    "Strictly one of queryable_ or orderedQueryable_ can " +
                    "have value, not both and not neither.");
            }
        }

        /// <summary>Sorts the elements of a sequence in descending order according to the selected key.</summary>
        public IQuery<TRecord> SortByDescending<TProperty>(Expression<Func<TRecord, TProperty>> keySelector)
        {
            if (queryable_ != null && orderedQueryable_ == null)
            {
                // Apply dataset constraint before ordering
                var queryableWithDataSetConstraint = collection_.DataSource.ApplyFinalConstraints(queryable_, loadFrom_);

                // First SortBy clause, use OrderBy of queryable_
                return new TemporalMongoQuery<TRecord>(collection_, loadFrom_, queryableWithDataSetConstraint.OrderByDescending(keySelector));
            }
            else if (queryable_ == null && orderedQueryable_ != null)
            {
                // Subsequent SortBy clauses, use ThenBy of the orderedQueryable_
                return new TemporalMongoQuery<TRecord>(collection_, loadFrom_, orderedQueryable_.ThenByDescending(keySelector));
            }
            else
            {
                throw new Exception(
                    "Strictly one of queryable_ or orderedQueryable_ can " +
                    "have value, not both and not neither.");
            }
        }

        /// <summary>Convert query to enumerable so iteration can be performed.</summary>
        public IEnumerable<TRecord> AsEnumerable()
        {
            IQueryable<TRecord> batchQueryable = null;
            if (queryable_ != null && orderedQueryable_ == null)
            {
                // Apply final constraints to queryable_ and assign to typedQueryable
                batchQueryable = collection_.DataSource.ApplyFinalConstraints(queryable_, loadFrom_);
            }
            else if (queryable_ == null && orderedQueryable_ != null)
            {
                // Assign orderedQueryable_ to typedQueryable, final constraints already applied
                batchQueryable = orderedQueryable_;
            }
            else
                throw new Exception(
                    "Either queryable_ or orderedQueryable_ must have value, but not both ");


            // Temporal query consists of three parts.
            //
            // * First, a typed query with filter and sort options specified by the caller
            //   is executed in batches, and its result is projected to a list of keys.
            // * Second, untyped query for all keys retrieved during the batch is executed
            //   and projected to (Id, DataSet, Key) elements only. Using Imports lookup
            //   sequence and FreezeImports flag, a list of TemporalIds for the latest
            //   record in the latest dataset is obtained. Records for which type does
            //   not match are skipped.
            // * Finally, typed query is executed for each of these Ids and the results
            //   are yield returned to the caller.
            //
            // Project to key instead of returning the entire record
            var projectedBatchQueryable = batchQueryable.Select(p => new RecordInfo {Id = p.Id, Key = p.Key});

            // Get enumerator for the query so we can pause at the end of each batch
            using (var stepOneEnumerator = projectedBatchQueryable.GetEnumerator())
            {
                int batchSize = 1000;
                bool continueQuery = true;
                while (continueQuery)
                {
                    // First step is to get all keys in this batch returned
                    // by the user specified query and sort order
                    int batchIndex = 0;
                    var batchKeysHashSet = new HashSet<string>();
                    var batchIdsHashSet = new HashSet<TemporalId>();
                    var batchIdsList = new List<TemporalId>();
                    while (true)
                    {
                        // Advance cursor and check if there are more results left in the query
                        continueQuery = stepOneEnumerator.MoveNext();

                        if (continueQuery)
                        {
                            // If yes, get key from the enumerator
                            RecordInfo recordInfo = stepOneEnumerator.Current;
                            string batchKey = recordInfo.Key;
                            TemporalId batchId = recordInfo.Id;

                            // Add Key and Id to hashsets, increment
                            // batch index only if this is a new key
                            if (batchKeysHashSet.Add(batchKey)) batchIndex++;
                            batchIdsHashSet.Add(batchId);
                            batchIdsList.Add(batchId);

                            // Break if batch size has been reached
                            if (batchIndex == batchSize) break;
                        }
                        else
                        {
                            // Otherwise exit from the while loop
                            break;
                        }
                    }

                    // We reach this point in the code if batch size is reached,
                    // or there are no more records in the query. Break from while
                    // loop if query is complete but there is nothing in the batch
                    if (!continueQuery && batchIndex == 0) break;

                    // The second step is to get (Id,DataSet,Key) for records with
                    // the specified keys using a query without type restriction.
                    //
                    // First, query base collection for records with keys in the hashset
                    IQueryable<Record> idQueryable = collection_.BaseCollection.AsQueryable()
                        .Where(p => batchKeysHashSet.Contains(p.Key));

                    // Apply the same final constraints (list of datasets, savedBy, etc.)
                    idQueryable = collection_.DataSource.ApplyFinalConstraints(idQueryable, loadFrom_);

                    // Apply ordering to get last object in last dataset for the keys
                    idQueryable = idQueryable
                        .OrderBy(p => p.Key) // _key
                        .ThenByDescending(p => p.DataSet) // _dataset
                        .ThenByDescending(p => p.Id); // _id

                    // Finally, project to (Id,DataSet,Key)
                    var projectedIdQueryable = idQueryable
                        .Select(p => new RecordInfo {Id = p.Id, DataSet = p.DataSet, Key = p.Key});

                    // Gets ImportsCutoffTime from the dataset detail record.
                    // Returns null if dataset detail record is not found.
                    TemporalId? importsCutoffTime = collection_.DataSource.GetImportsCutoffTime(loadFrom_);

                    // Create a list of TemporalIds for the records obtained using
                    // dataset lookup rules for the keys in the batch
                    var recordIds = new List<TemporalId>();
                    string currentKey = null;
                    foreach (var obj in projectedIdQueryable)
                    {
                        string objKey = obj.Key;
                        if (currentKey == objKey)
                        {
                            // The key was encountered before. Because the data is sorted by
                            // key and then by dataset and ID, this indicates that the object
                            // is not the latest and can be skipped
                            continue;
                        }
                        else
                        {
                            TemporalId recordId = obj.Id;
                            TemporalId recordDataSet = obj.DataSet;

                            // Include the record if one of the following is true:
                            // 
                            // * ImportsCutoffTime is not set
                            // * ImportsCutoffTime does not apply because the record
                            //   is in the dataset itself, not its Imports list
                            // * The record is in the list of Imports, and its TemporalId
                            //   is earlier than ImportsCutoffTime
                            if (importsCutoffTime == null
                                || recordDataSet == loadFrom_
                                || recordId < importsCutoffTime)
                            {
                                // Iterating over the dataset lookup list in descending order,
                                // we reached dataset of the record before finding a dataset
                                // that is earlier than the record. This is the latest record
                                // in the latest dataset for this key subject to the freeze rule.

                                // Take the first object for a new key, relying on sorting
                                // by dataset and then by record's TemporalId in descending
                                // order.
                                currentKey = objKey;

                                // Add to dictionary only if found in the list of batch Ids
                                // Otherwise this is not the latest record in the latest
                                // dataset (subject to freeze rule) and it should be skipped.
                                if (batchIdsHashSet.Contains(recordId))
                                {
                                    recordIds.Add(recordId);
                                }
                            }
                        }
                    }

                    // If the list of record Ids is empty, continue
                    if (recordIds.Count == 0) break;

                    // Finally, retrieve the records only for the Ids in the list
                    //
                    // Create a typed queryable
                    IQueryable<TRecord> recordQueryable = collection_.TypedCollection.AsQueryable()
                        .Where(p => recordIds.Contains(p.Id));

                    // Populate a dictionary of records by Id
                    var recordDict = new Dictionary<TemporalId, TRecord>();
                    foreach (var record in recordQueryable)
                    {
                        recordDict.Add(record.Id, record);
                    }

                    // Using the list ensures that records are returned
                    // in the same order as the original query
                    foreach(var batchId in batchIdsList)
                    {
                        // If a record TemporalId is present in batchIds but not
                        // in recordDict, this indicates that the record found
                        // by the query is not the latest and it should be skipped
                        if (recordDict.TryGetValue(batchId, out var result))
                        {
                            // Yield return the result
                            yield return result;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Displays MongoDB aggregation pipeline JSON for server-executed queries
        /// starting from ``{aggregate...''.
        ///
        /// ATTENTION - if this method does not show aggregation pipeline JSON
        /// starting from ``{aggregate...'' and  instead shows class names, the query
        /// will not execute on the server and should be revised.
        /// </summary>
        public override string ToString()
        {
            // If in ordered pipeline phase, return orderedQueryable_;
            // otherwise return queryable_
            if (orderedQueryable_ != null) return orderedQueryable_.ToString();
            else return queryable_.ToString();
        }
    }
}
