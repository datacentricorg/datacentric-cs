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
using System.Net.Mime;
using DataCentric;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Xunit;

namespace DataCentric.Test
{
    /// <summary>Unit test for TemporalMongoDataSource.</summary>
    public class TemporalMongoTest : UnitTest
    {
        /// <summary>Smoke test.</summary>
        [Fact]
        public void Smoke()
        {
            using (var context = CreateMethodContext())
            {
                SaveBasicData(context);

                // Get dataset identifiers
                var dataSet0 = context.GetDataSet("DataSet0", context.DataSet);
                var dataSet1 = context.GetDataSet("DataSet1", context.DataSet);

                // Create keys
                var keyA0 = new BaseSampleKey()
                {
                    RecordName = "A",
                    RecordIndex = 0
                };
                var keyB0 = new BaseSampleKey()
                {
                    RecordName = "B",
                    RecordIndex = 0
                };

                // Verify the result of loading records from datasets A and B
                VerifyLoad(context, "DataSet0", keyA0);
                VerifyLoad(context, "DataSet1", keyA0);
                VerifyLoad(context, "DataSet0", keyB0);
                VerifyLoad(context, "DataSet1", keyB0);
            }
        }

        /// <summary>Test for the query across multiple datasets.</summary>
        [Fact]
        public void MultipleDataSetQuery()
        {
            using (var context = CreateMethodContext())
            {
                // Begin from DataSet0
                var dataSet0 = context.CreateDataSet("DataSet0", context.DataSet);

                // Create initial version of the records
                SaveMinimalRecord(context, "DataSet0", "A", 0, 0);
                SaveMinimalRecord(context, "DataSet0", "B", 1, 0);
                SaveMinimalRecord(context, "DataSet0", "A", 2, 0);
                SaveMinimalRecord(context, "DataSet0", "B", 3, 0);

                // Create second version of some records
                SaveMinimalRecord(context, "DataSet0", "A", 0, 1);
                SaveMinimalRecord(context, "DataSet0", "B", 1, 1);
                SaveMinimalRecord(context, "DataSet0", "A", 2, 1);
                SaveMinimalRecord(context, "DataSet0", "B", 3, 1);

                // Create third version of even fewer records
                SaveMinimalRecord(context, "DataSet0", "A", 0, 2);
                SaveMinimalRecord(context, "DataSet0", "B", 1, 2);
                SaveMinimalRecord(context, "DataSet0", "A", 2, 2);
                SaveMinimalRecord(context, "DataSet0", "B", 3, 2);

                // Same in DataSet1
                var dataSet1 = context.CreateDataSet("DataSet1", new TemporalId[] { dataSet0 }, context.DataSet);

                // Create initial version of the records
                SaveMinimalRecord(context, "DataSet1", "A", 4, 0);
                SaveMinimalRecord(context, "DataSet1", "B", 5, 0);
                SaveMinimalRecord(context, "DataSet1", "A", 6, 0);
                SaveMinimalRecord(context, "DataSet1", "B", 7, 0);

                // Create second version of some records
                SaveMinimalRecord(context, "DataSet1", "A", 4, 1);
                SaveMinimalRecord(context, "DataSet1", "B", 5, 1);
                SaveMinimalRecord(context, "DataSet1", "A", 6, 1);
                SaveMinimalRecord(context, "DataSet1", "B", 7, 1);

                // Next in DataSet2
                var dataSet2 = context.CreateDataSet("DataSet2", new TemporalId[] { dataSet0 }, context.DataSet);
                SaveMinimalRecord(context, "DataSet2", "A", 8, 0);
                SaveMinimalRecord(context, "DataSet2", "B", 9, 0);

                // Next in DataSet3
                var dataSet3 = context.CreateDataSet("DataSet3", new TemporalId[] { dataSet0, dataSet1, dataSet2 }, context.DataSet);
                SaveMinimalRecord(context, "DataSet3", "A", 10, 0);
                SaveMinimalRecord(context, "DataSet3", "B", 11, 0);

                // Query for RecordName=B
                var query = context.GetQuery<BaseSample>(dataSet3)
                    .Where(p => p.RecordName == "B")
                    .SortBy(p => p.RecordName)
                    .SortBy(p => p.RecordIndex)
                    .AsEnumerable();

                foreach (var obj in query)
                {
                    var dataSetName = context.LoadOrNull<DataSet>(obj.DataSet).DataSetName;
                    context.Log.Verify($"Key={obj.Key} DataSet={dataSetName} Version={obj.Version}");
                }
            }
        }

        /// <summary>Test of CreateOrderedTemporalId method.</summary>
        [Fact]
        public void CreateOrderedTemporalId()
        {
            using (var context = CreateMethodContext())
            {
                for (int i = 0; i < 10_000; ++i)
                {
                    // Invoke 10,000 times to ensure there is no log report of non-increasing TemporalId
                    context.DataSource.CreateOrderedTemporalId();
                }

                // Confirm no log output indicating non-increasing id from inside the for loop
                context.Log.Verify("Log should have no lines before this one.");
            }
        }

        /// <summary>Test DeletedRecord.</summary>
        [Fact]
        public void Delete()
        {
            using (var context = CreateMethodContext())
            {
                SaveBasicData(context);

                // Get dataset identifiers
                var dataSet0 = context.GetDataSet("DataSet0", context.DataSet);
                var dataSet1 = context.GetDataSet("DataSet1", context.DataSet);

                // Create keys
                var keyA0 = new BaseSampleKey()
                {
                    RecordName = "A",
                    RecordIndex = 0
                };
                var keyB0 = new BaseSampleKey()
                {
                    RecordName = "B",
                    RecordIndex = 0
                };

                // Verify the result of loading records from datasets A and B
                context.Log.Verify("Initial load");
                VerifyLoad(context, "DataSet0", keyA0);
                VerifyLoad(context, "DataSet1", keyA0);
                VerifyLoad(context, "DataSet0", keyB0);
                VerifyLoad(context, "DataSet1", keyB0);

                context.Log.Verify("Query in dataset DataSet0");
                VerifyQuery<BaseSample>(context, "DataSet0");
                context.Log.Verify("Query in dataset DataSet1");
                VerifyQuery<BaseSample>(context, "DataSet1");

                context.Log.Verify("Delete A0 record in B dataset");
                context.Delete(keyA0, dataSet1);
                VerifyLoad(context, "DataSet0", keyA0);
                VerifyLoad(context, "DataSet1", keyA0);

                context.Log.Verify("Query in dataset DataSet0");
                VerifyQuery<BaseSample>(context, "DataSet0");
                context.Log.Verify("Query in dataset DataSet1");
                VerifyQuery<BaseSample>(context, "DataSet1");

                context.Log.Verify("Delete A0 record in A dataset");
                context.Delete(keyA0, dataSet0);
                VerifyLoad(context, "DataSet0", keyA0);
                VerifyLoad(context, "DataSet1", keyA0);

                context.Log.Verify("Query in dataset DataSet0");
                VerifyQuery<BaseSample>(context, "DataSet0");
                context.Log.Verify("Query in dataset DataSet1");
                VerifyQuery<BaseSample>(context, "DataSet1");

                context.Log.Verify("Delete B0 record in B dataset");
                context.Delete(keyB0, dataSet1);
                VerifyLoad(context, "DataSet0", keyB0);
                VerifyLoad(context, "DataSet1", keyB0);

                context.Log.Verify("Query in dataset DataSet0");
                VerifyQuery<BaseSample>(context, "DataSet0");
                context.Log.Verify("Query in dataset DataSet1");
                VerifyQuery<BaseSample>(context, "DataSet1");
            }
        }

        /// <summary>
        /// Test saving object of a different type for the same key.
        ///
        /// The objective of this test is to confirm that LoadOrNull
        /// for the wrong type will give an error, while query will
        /// skip the object of the wrong type even if there is an
        /// earlier version of the object with the same key that
        /// has a compatible type.
        /// </summary>
        [Fact]
        public void TypeChange()
        {
            using (var context = CreateMethodContext())
            {
                // Create records with minimal data

                var dataSet0 = context.CreateDataSet("DataSet0", context.DataSet);
                SaveDerivedRecord(context, "DataSet0", "A", 0);

                var dataSet1 = context.CreateDataSet("DataSet1", new TemporalId[] { dataSet0 }, context.DataSet);
                SaveDerivedFromDerivedRecord(context, "DataSet1", "B", 0);

                // Create keys
                var keyA0 = new BaseSampleKey()
                {
                    RecordName = "A",
                    RecordIndex = 0
                };
                var keyB0 = new BaseSampleKey()
                {
                    RecordName = "B",
                    RecordIndex = 0
                };

                // Verify the result of loading records from datasets A and B
                context.Log.Verify("Initial load");
                VerifyLoad(context, "DataSet0", keyA0);
                VerifyLoad(context, "DataSet1", keyA0);
                VerifyLoad(context, "DataSet0", keyB0);
                VerifyLoad(context, "DataSet1", keyB0);

                context.Log.Verify("Query in dataset DataSet0 for type DerivedSample");
                VerifyQuery<DerivedSample>(context, "DataSet0");
                context.Log.Verify("Query in dataset DataSet1 for type DerivedSample");
                VerifyQuery<DerivedSample>(context, "DataSet1");

                context.Log.Verify("Change A0 record type in B dataset to C");
                SaveOtherDerivedRecord(context, "DataSet1", "A", 0);
                VerifyLoad(context, "DataSet0", keyA0);
                VerifyLoad(context, "DataSet1", keyA0);

                context.Log.Verify("Query in dataset DataSet0 for type DerivedSample");
                VerifyQuery<DerivedSample>(context, "DataSet0");
                context.Log.Verify("Query in dataset DataSet1 for type DerivedSample");
                VerifyQuery<DerivedSample>(context, "DataSet1");

                context.Log.Verify("Change A0 record type in A dataset to C");
                SaveOtherDerivedRecord(context, "DataSet0", "A", 0);
                VerifyLoad(context, "DataSet0", keyA0);
                VerifyLoad(context, "DataSet1", keyA0);

                context.Log.Verify("Query in dataset DataSet0 for type DerivedSample");
                VerifyQuery<DerivedSample>(context, "DataSet0");
                context.Log.Verify("Query in dataset DataSet1 for type DerivedSample");
                VerifyQuery<DerivedSample>(context, "DataSet1");

                context.Log.Verify("Change B0 record type in B dataset to C");
                SaveOtherDerivedRecord(context, "DataSet1", "B", 0);
                VerifyLoad(context, "DataSet0", keyB0);
                VerifyLoad(context, "DataSet1", keyB0);

                context.Log.Verify("Query in dataset DataSet0 for type DerivedSample");
                VerifyQuery<DerivedSample>(context, "DataSet0");
                context.Log.Verify("Query in dataset DataSet1 for type DerivedSample");
                VerifyQuery<DerivedSample>(context, "DataSet1");
            }
        }

        /// <summary>Test query.</summary>
        [Fact]
        public void ElementTypesQuery()
        {
            using (var context = CreateMethodContext())
            {
                // Saves data in A and B datasets, A is an import of B
                SaveCompleteData(context);

                // Look in B dataset
                var dataSet1 = context.GetDataSet("DataSet1", context.DataSet);
                var testQuery = context.GetQuery<DerivedSample>(dataSet1)
                    .Where(p => p.DataElementList[0].DoubleElement3 == 1.0)
                    .Where(p => p.DataElementList[0].StringElement3 == "A0")
                    .Where(p => p.LocalDateElement < new LocalDate(2003, 5, 2))
                    .Where(p => p.LocalDateElement > new LocalDate(2003, 4, 30))
                    .Where(p => p.LocalDateElement == new LocalDate(2003, 5, 1))
                    .Where(p => p.LocalTimeElement < new LocalTime(10, 15, 31))
                    .Where(p => p.LocalTimeElement > new LocalTime(10, 15, 29))
                    .Where(p => p.LocalTimeElement == new LocalTime(10, 15, 30))
                    .Where(p => p.LocalDateTimeElement < new LocalDateTime(2003, 5, 1, 10, 15, 01))
                    .Where(p => p.LocalDateTimeElement > new LocalDateTime(2003, 5, 1, 10, 14, 59))
                    .Where(p => p.LocalDateTimeElement == new LocalDateTime(2003, 5, 1, 10, 15))
                    .Where(p => p.InstantElement < new LocalDateTime(2003, 5, 1, 10, 15, 01).ToInstant(DateTimeZone.Utc))
                    .Where(p => p.InstantElement > new LocalDateTime(2003, 5, 1, 10, 14, 59).ToInstant(DateTimeZone.Utc))
                    .Where(p => p.InstantElement == new LocalDateTime(2003, 5, 1, 10, 15).ToInstant(DateTimeZone.Utc))
                    .Where(p => p.StringElement2 == String.Empty)
                    .Where(p => p.KeyElement == new BaseSampleKey() {RecordName = "BB", RecordIndex = 2})
                    .SortBy(p => p.RecordName);

                foreach (var obj in testQuery.AsEnumerable())
                {
                    context.Log.Verify($"Key={obj.Key} Type={obj.GetType().Name}");
                }
            }
        }

        /// <summary>Test polymorphic load and query.</summary>
        [Fact]
        public void PolymorphicQuery()
        {
            using (var context = CreateMethodContext())
            {
                // Saves data in A and B datasets, A is an import of B
                SaveCompleteData(context);

                // Look in B dataset
                var dataSet3 = context.GetDataSet("DataSet3", context.DataSet);

                // Load record of derived types by base
                context.Log.Verify("Load all records by key as MongoTestData.");
                {
                    var key = new BaseSampleKey {RecordName = "A", RecordIndex = 0};
                    var obj = context.LoadOrNull(key, dataSet3);
                    context.Log.Verify($"    Key={obj.Key} Type={obj.GetType().Name}");
                }
                {
                    var key = new BaseSampleKey { RecordName = "B", RecordIndex = 0 };
                    var obj = context.LoadOrNull(key, dataSet3);
                    context.Log.Verify($"    Key={obj.Key} Type={obj.GetType().Name}");
                }
                {
                    var key = new BaseSampleKey { RecordName = "C", RecordIndex = 0 };
                    var obj = context.LoadOrNull(key, dataSet3);
                    context.Log.Verify($"    Key={obj.Key} Type={obj.GetType().Name}");
                }
                {
                    var key = new BaseSampleKey { RecordName = "D", RecordIndex = 0 };
                    var obj = context.LoadOrNull(key, dataSet3);
                    context.Log.Verify($"    Key={obj.Key} Type={obj.GetType().Name}");
                }
                {
                    context.Log.Verify("Query by MongoTestData, unconstrained");
                    var query = context.GetQuery<BaseSample>(dataSet3).SortBy(p => p.RecordName).SortBy(p => p.RecordIndex);
                    foreach (var obj in query.AsEnumerable())
                    {
                        context.Log.Verify($"    Key={obj.Key} Type={obj.GetType().Name}");
                    }
                }
                {
                    context.Log.Verify("Query by DerivedSample : BaseSample which also picks up DerivedFromDerivedSample : DerivedSample, unconstrained");
                    var query = context.GetQuery<DerivedSample>(dataSet3).SortBy(p => p.RecordName).SortBy(p => p.RecordIndex);
                    foreach (var obj in query.AsEnumerable())
                    {
                        context.Log.Verify($"    Key={obj.Key} Type={obj.GetType().Name}");
                    }
                }
                {
                    context.Log.Verify("Query by OtherDerivedSample : BaseSample, unconstrained");
                    var query = context.GetQuery<OtherDerivedSample>(dataSet3).SortBy(p => p.RecordName).SortBy(p => p.RecordIndex);
                    foreach (var obj in query.AsEnumerable())
                    {
                        context.Log.Verify($"    Key={obj.Key} Type={obj.GetType().Name}");
                    }
                }
                {
                    context.Log.Verify("Query by DerivedFromDerivedSample : DerivedSample, where DerivedSample : BaseSample, unconstrained");
                    var query = context.GetQuery<DerivedFromDerivedSample>(dataSet3).SortBy(p => p.RecordName).SortBy(p => p.RecordIndex);
                    foreach (var obj in query.AsEnumerable())
                    {
                        context.Log.Verify($"    Key={obj.Key} Type={obj.GetType().Name}");
                    }
                }
            }
        }

        /// <summary>Test sorting.</summary>
        [Fact]
        public void Sort()
        {
            using (var context = CreateMethodContext())
            {
                // Saves data in A and B datasets, A is an import of B
                SaveCompleteData(context);

                // Look in B dataset
                var dataSet3 = context.GetDataSet("DataSet3", context.DataSet);

                context.Log.Verify("Query by MongoTestData, sort by RecordIndex descending, then by DoubleElement ascending");
                var baseQuery = context.GetQuery<BaseSample>(dataSet3)
                    .SortByDescending(p => p.RecordIndex)
                    .SortBy(p => p.DoubleElement);
                foreach (var obj in baseQuery.AsEnumerable())
                {
                    context.Log.Verify(
                        $"    RecordIndex={obj.RecordIndex} DoubleElement={obj.DoubleElement} " +
                        $"Key={obj.Key} Type={obj.GetType().Name}");
                }
            }
        }

        /// <summary>Test for CutoffTime filtering.</summary>
        [Fact]
        public void CutoffTime()
        {
            using (var context = CreateMethodContext())
            {
                // Create two versions in DataSet0
                var dataSet0 = context.CreateDataSet("DataSet0", context.DataSet);
                TemporalId objA0 = SaveMinimalRecord(context, "DataSet0", "A", 0, 0);
                TemporalId objA1 = SaveMinimalRecord(context, "DataSet0", "A", 0, 1);

                // Create two versions in DataSet1
                var dataSet1 = context.CreateDataSet("DataSet1", new TemporalId[] { dataSet0 }, context.DataSet);
                TemporalId objB0 = SaveMinimalRecord(context, "DataSet1", "B", 0, 0);
                TemporalId objB1 = SaveMinimalRecord(context, "DataSet1", "B", 0, 1);

                TemporalId cutoffTemporalId = context.DataSource.CreateOrderedTemporalId();

                // Create third version of the records
                TemporalId objA2 = SaveMinimalRecord(context, "DataSet0", "A", 0, 2);
                TemporalId objB2 = SaveMinimalRecord(context, "DataSet1", "B", 0, 2);

                // Create new records that did not exist before
                TemporalId objC0 = SaveMinimalRecord(context, "DataSet0", "C", 0, 0);
                TemporalId objD0 = SaveMinimalRecord(context, "DataSet1", "D", 0, 0);

                // Load each record by TemporalId
                context.Log.Verify("Load records by TemporalId without constraint");
                context.Log.Verify($"Found for TemporalId(A0)={context.LoadOrNull<BaseSample>(objA0) != null}");
                context.Log.Verify($"Found for TemporalId(A1)={context.LoadOrNull<BaseSample>(objA1) != null}");
                context.Log.Verify($"Found for TemporalId(A2)={context.LoadOrNull<BaseSample>(objA2) != null}");
                context.Log.Verify($"Found for TemporalId(C0)={context.LoadOrNull<BaseSample>(objC0) != null}");

                // Load each record by string key
                if (true)
                {
                    var loadedA0 = context.LoadOrNull(new BaseSampleKey() {RecordName = "A", RecordIndex = 0}, dataSet1);
                    var loadedC0 = context.LoadOrNull(new BaseSampleKey() {RecordName = "C", RecordIndex = 0}, dataSet1);

                    context.Log.Verify("Load records by string key without constraint");
                    if (loadedA0 != null) context.Log.Verify($"    Version found for key=A;0: {loadedA0.Version}");
                    if (loadedC0 != null) context.Log.Verify($"    Version found for key=C;0: {loadedC0.Version}");
                }

                // Query for all records
                if (true)
                {
                    var query = context.GetQuery<BaseSample>(dataSet1)
                        .SortBy(p => p.RecordName)
                        .SortBy(p => p.RecordIndex)
                        .AsEnumerable();

                    context.Log.Verify("Query records without constraint");
                    foreach (var obj in query)
                    {
                        var dataSetName = context.LoadOrNull<DataSet>(obj.DataSet).DataSetName;
                        context.Log.Verify($"    Key={obj.Key} DataSet={dataSetName} Version={obj.Version}");
                    }
                }

                // Set revision time constraint
                context.DataSource.CastTo<TemporalMongoDataSource>().CutoffTime = cutoffTemporalId;

                // Get each record by TemporalId
                context.Log.Verify("Load records by TemporalId with CutoffTime constraint");
                context.Log.Verify($"Found for TemporalId(A0)={context.LoadOrNull<BaseSample>(objA0) != null}");
                context.Log.Verify($"Found for TemporalId(A1)={context.LoadOrNull<BaseSample>(objA1) != null}");
                context.Log.Verify($"Found for TemporalId(A2)={context.LoadOrNull<BaseSample>(objA2) != null}");
                context.Log.Verify($"Found for TemporalId(C0)={context.LoadOrNull<BaseSample>(objC0) != null}");

                // Load each record by string key
                if (true)
                {
                    var loadedA0 = context.LoadOrNull(new BaseSampleKey() { RecordName = "A", RecordIndex = 0 }, dataSet1);
                    var loadedC0 = context.LoadOrNull(new BaseSampleKey() { RecordName = "C", RecordIndex = 0 }, dataSet1);

                    context.Log.Verify("Load records by string key with CutoffTime constraint");
                    if (loadedA0 != null) context.Log.Verify($"    Version found for key=A;0: {loadedA0.Version}");
                    if (loadedC0 != null) context.Log.Verify($"    Version found for key=C;0: {loadedC0.Version}");
                }

                // Query for revised before the cutoff time
                if (true)
                {
                    var query = context.GetQuery<BaseSample>(dataSet1)
                        .SortBy(p => p.RecordName)
                        .SortBy(p => p.RecordIndex)
                        .AsEnumerable();

                    context.Log.Verify("Query records with CutoffTime constraint");
                    foreach (var obj in query)
                    {
                        var dataSetName = context.LoadOrNull<DataSet>(obj.DataSet).DataSetName;
                        context.Log.Verify($"    Key={obj.Key} DataSet={dataSetName} Version={obj.Version}");
                    }
                }

                // Clear revision time constraint before exiting to avoid an error
                // about deleting readonly database. The error occurs because
                // revision time constraint makes the data source readonly.
                context.DataSource.CastTo<TemporalMongoDataSource>().CutoffTime = null;
            }
        }

        /// <summary>Test for the query across deleted entities.</summary>
        [Fact]
        public void QueryWithFilterOnDeletedRecord()
        {
            using (var context = CreateMethodContext())
            {
                // Create datasets
                var dataSet0 = context.CreateDataSet("DataSet0", context.DataSet);

                // Create initial version of the records
                context.Log.Verify("Write A;0 record to A dataset");
                SaveMinimalRecord(context, "DataSet0", "A", 0, 1);

                var keyA0 = new BaseSampleKey
                {
                    RecordName = "A",
                    RecordIndex = 0
                };

                // Load from storage before deletion
                if (true)
                {
                    var loadedRecord = context.LoadOrNull(keyA0, dataSet0);
                    context.Log.Assert(loadedRecord != null, "Record found before deletion assert.");
                }

                // Query before deletion
                if (true)
                {
                    var query = context.GetQuery<BaseSample>(dataSet0)
                        .Where(p => p.Version == 1)
                        .AsEnumerable();
                    int recordCount = 0;
                    foreach (var obj in query)
                    {
                        var dataSetName = context.LoadOrNull<DataSet>(obj.DataSet).DataSetName;
                        context.Log.Verify($"Query returned Key={obj.Key} DataSet={dataSetName} Version={obj.Version}");
                        recordCount++;
                    }

                    context.Log.Verify($"Query record count before deletion {recordCount}");
                }

                context.Log.Verify("Delete A;0 record in A dataset");
                context.Delete(keyA0, dataSet0);

                // Load from storage before deletion
                if (true)
                {
                    var loadedRecord = context.LoadOrNull(keyA0, dataSet0);
                    context.Log.Assert(loadedRecord == null, "Record not found after deletion assert.");
                }

                // Query after deletion
                if (true)
                {
                    var query = context.GetQuery<BaseSample>(dataSet0)
                        .Where(p => p.Version == 1)
                        .AsEnumerable();
                    int recordCount = 0;
                    foreach (var obj in query)
                    {
                        var dataSetName = context.LoadOrNull<DataSet>(obj.DataSet).DataSetName;
                        context.Log.Verify($"Query returned Key={obj.Key} DataSet={dataSetName} Version={obj.Version}");
                        recordCount++;
                    }

                    context.Log.Verify($"Query record count after deletion {recordCount}");
                }
            }
        }

        /// <summary>Load the object and verify the outcome.</summary>
        private void VerifyLoad<TKey, TRecord>(IContext context, string dataSetName, TypedKey<TKey, TRecord> key)
            where TKey : TypedKey<TKey, TRecord>, new()
            where TRecord : TypedRecord<TKey, TRecord>
        {
            // Get dataset and try loading the record
            var dataSet = context.GetDataSet(dataSetName, context.DataSet);
            TRecord record = context.LoadOrNull(key, dataSet);

            if (record == null)
            {
                // Not found
                context.Log.Verify($"Record {key} in dataset {dataSetName} not found.");
            }
            else
            {
                // Found, also checks that the key matches
                Assert.True(record.Key == key.ToString(),
                    $"Record found for key={key} in dataset {dataSetName} " +
                    $"has wrong key record.Key={record.Key}");
                context.Log.Verify(
                    $"Record {key} in dataset {dataSetName} found and " +
                    $"has Type={record.GetType().Name}.");
            }
        }

        /// <summary>Query over all records of the specified type in the specified dataset.</summary>
        private void VerifyQuery<TRecord>(IContext context, string dataSetName)
            where TRecord : Record
        {
            // Get dataset and query
            var dataSet = context.GetDataSet(dataSetName, context.DataSet);
            var query = context.GetQuery<TRecord>(dataSet);

            // Iterate over records
            foreach (var record in query.AsEnumerable())
            {
                context.Log.Verify(
                    $"Record {record.Key} returned by query in dataset {dataSetName} and " +
                    $"has Type={record.GetType().Name}.");
            }
        }

        /// <summary>Two datasets and two objects, one base and one derived.</summary>
        private void SaveBasicData(IContext context)
        {
            // Create first dataset and record
            var dataSet0 = context.CreateDataSet("DataSet0", context.DataSet);
            SaveBaseRecord(context, "DataSet0", "A", 0);

            // Create second dataset and record, first record will be visible in both
            var dataSet1 = context.CreateDataSet("DataSet1", new TemporalId[] {dataSet0}, context.DataSet);
            SaveDerivedRecord(context, "DataSet1", "B", 0);
        }

        /// <summary>Two datasets and eight objects, split between base and derived.</summary>
        private void SaveCompleteData(IContext context)
        {
            // Create records with minimal data

            var dataSet0 = context.CreateDataSet("DataSet0", context.DataSet);
            SaveBaseRecord(context, "DataSet0", "A", 0);
            SaveBaseRecord(context, "DataSet0", "A", 1);
            SaveBaseRecord(context, "DataSet0", "A", 2);
            SaveBaseRecord(context, "DataSet0", "A", 3);

            var dataSet1 = context.CreateDataSet("DataSet1", new TemporalId[] { dataSet0 }, context.DataSet);
            SaveDerivedRecord(context, "DataSet1", "B", 0);
            SaveDerivedRecord(context, "DataSet1", "B", 1);
            SaveDerivedRecord(context, "DataSet1", "B", 2);
            SaveDerivedRecord(context, "DataSet1", "B", 3);

            var dataSet2 = context.CreateDataSet("DataSet2", new TemporalId[] { dataSet0 }, context.DataSet);
            SaveOtherDerivedRecord(context, "DataSet2", "C", 0);
            SaveOtherDerivedRecord(context, "DataSet2", "C", 1);
            SaveOtherDerivedRecord(context, "DataSet2", "C", 2);
            SaveOtherDerivedRecord(context, "DataSet2", "C", 3);

            var dataSet3 = context.CreateDataSet("DataSet3", new TemporalId[] { dataSet0, dataSet1, dataSet2 }, context.DataSet);
            SaveDerivedFromDerivedRecord(context, "DataSet3", "D", 0);
            SaveDerivedFromDerivedRecord(context, "DataSet3", "D", 1);
            SaveDerivedFromDerivedRecord(context, "DataSet3", "D", 2);
            SaveDerivedFromDerivedRecord(context, "DataSet3", "D", 3);
        }

        /// <summary>Minimal data in multiple datasets with overlapping imports.</summary>
        private void SaveMultiDataSetData(IContext context)
        {
            // Create datasets
            var dataSet0 = context.CreateDataSet("DataSet0", context.DataSet);
            var dataSet1 = context.CreateDataSet("DataSet1", new TemporalId[] {dataSet0}, context.DataSet);
            var dataSet2 = context.CreateDataSet("DataSet2", new TemporalId[] {dataSet0}, context.DataSet);
            var dataSet3 = context.CreateDataSet("DataSet3", new TemporalId[] {dataSet0, dataSet1, dataSet2}, context.DataSet);

            // Create records
            SaveMinimalRecord(context, "DataSet0", "A", 0);
            SaveMinimalRecord(context, "DataSet0", "A", 1);
            SaveMinimalRecord(context, "DataSet1", "B", 0);
            SaveMinimalRecord(context, "DataSet1", "B", 1);
            SaveMinimalRecord(context, "DataSet2", "C", 0);
            SaveMinimalRecord(context, "DataSet2", "C", 1);
            SaveMinimalRecord(context, "DataSet3", "D", 0);
            SaveMinimalRecord(context, "DataSet3", "D", 1);
        }

        /// <summary>Save record with minimal data for testing how the records are found. </summary>
        private TemporalId SaveMinimalRecord(IContext context, string dataSetName, string recordName, int recordIndex, int? version = null)
        {
            var rec = new BaseSample();
            rec.RecordName = recordName;
            rec.RecordIndex = recordIndex;
            rec.Version = version;

            var dataSet = context.GetDataSet(dataSetName, context.DataSet);
            context.SaveOne(rec, dataSet);

            return rec.Id;
        }

        /// <summary>Save base record</summary>
        private TemporalId SaveBaseRecord(IContext context, string dataSetName, string recordName, int recordIndex)
        {
            var rec = new BaseSample();
            rec.RecordName = recordName;
            rec.RecordIndex = recordIndex;
            rec.DoubleElement = 100.0;
            rec.LocalDateElement = new LocalDate(2003, 5, 1);
            rec.LocalTimeElement = new LocalTime(10, 15, 30); // 10:15:30
            rec.LocalMinuteElement = new LocalMinute(10, 15); // 10:15
            rec.LocalDateTimeElement = new LocalDateTime(2003, 5, 1, 10, 15); // 2003-05-01T10:15:
            rec.InstantElement = new LocalDateTime(2003, 5, 1, 10, 15).ToInstant(DateTimeZone.Utc); // 2003-05-01T10:15:00
            rec.EnumValue = SampleEnum.EnumValue2;

            var dataSet = context.GetDataSet(dataSetName, context.DataSet);
            context.SaveOne(rec, dataSet);
            return rec.Id;
        }

        /// <summary>Save derived record</summary>
        private TemporalId SaveDerivedRecord(IContext context, string dataSetName, string recordName, int recordIndex)
        {
            var rec = new DerivedSample();
            rec.RecordName = recordName;
            rec.RecordIndex = recordIndex;
            rec.DoubleElement = 300.0;
            rec.LocalDateElement = new LocalDate(2003, 5, 1);
            rec.LocalTimeElement = new LocalTime(10, 15, 30); // 10:15:30
            rec.LocalMinuteElement = new LocalMinute(10, 15); // 10:15
            rec.LocalDateTimeElement = new LocalDateTime(2003, 5, 1, 10, 15); // 2003-05-01T10:15:00
            rec.InstantElement = new LocalDateTime(2003, 5, 1, 10, 15).ToInstant(DateTimeZone.Utc); // 2003-05-01T10:15:00
            rec.StringElement2 = String.Empty; // Test how empty value is recorded
            rec.DoubleElement2 = 200.0;

            // String collections
            rec.ArrayOfString = new string[] { "A", "B", "C" };
            rec.ListOfString = new List<string>() { "A", "B", "C" };

            // Double collections
            rec.ArrayOfDouble = new double[] { 1.0, 2.0, 3.0 };
            rec.ListOfDouble = new List<double>() { 1.0, 2.0, 3.0 };
            rec.ListOfNullableDouble = new List<double?>();
            rec.ListOfNullableDouble.Add(10.0);
            rec.ListOfNullableDouble.Add(null);
            rec.ListOfNullableDouble.Add(30.0);

            // Data element
            rec.DataElement = new ElementSample();
            rec.DataElement.DoubleElement3 = 1.0;
            rec.DataElement.StringElement3 = "AA";

            // Data element list
            rec.DataElementList = new List<ElementSample>();
            var elementList0 = new ElementSample();
            elementList0.DoubleElement3 = 1.0;
            elementList0.StringElement3 = "A0";
            rec.DataElementList.Add(elementList0);
            var elementList1 = new ElementSample();
            elementList1.DoubleElement3 = 2.0;
            elementList1.StringElement3 = "A1";
            rec.DataElementList.Add(elementList1);

            // Key element
            rec.KeyElement = new BaseSampleKey();
            rec.KeyElement.RecordName = "BB";
            rec.KeyElement.RecordIndex = 2;

            // Key element list
            rec.KeyElementList = new List<BaseSampleKey>();
            var keyList0 = new BaseSampleKey();
            keyList0.RecordName = "B0";
            keyList0.RecordIndex = 3;
            rec.KeyElementList.Add(keyList0);
            var keyList1 = new BaseSampleKey();
            keyList1.RecordName = "B1";
            keyList1.RecordIndex = 4;
            rec.KeyElementList.Add(keyList1);

            var dataSet = context.GetDataSet(dataSetName, context.DataSet);
            context.SaveOne(rec, dataSet);
            return rec.Id;
        }

        /// <summary>Save other derived record.</summary>
        private TemporalId SaveOtherDerivedRecord(IContext context, string dataSetName, string recordName, int recordIndex)
        {
            var rec = new OtherDerivedSample();
            rec.RecordName = recordName;
            rec.RecordIndex = recordIndex;
            rec.DoubleElement = 300.0;
            rec.LocalDateElement = new LocalDate(2003, 5, 1);
            rec.LocalTimeElement = new LocalTime(10, 15, 30); // 10:15:30
            rec.LocalMinuteElement = new LocalMinute(10, 15); // 10:15
            rec.LocalDateTimeElement = new LocalDateTime(2003, 5, 1, 10, 15); // 2003-05-01T10:15:00
            rec.InstantElement = new LocalDateTime(2003, 5, 1, 10, 15).ToInstant(DateTimeZone.Utc); // 2003-05-01T10:15:00
            rec.OtherStringElement2 = String.Empty; // Test how empty value is recorded
            rec.OtherDoubleElement2 = 200.0;

            var dataSet = context.GetDataSet(dataSetName, context.DataSet);
            context.SaveOne(rec, dataSet);
            return rec.Id;
        }

        /// <summary>Save record that is derived from derived.</summary>
        private TemporalId SaveDerivedFromDerivedRecord(IContext context, string dataSetName, string recordName, int recordIndex)
        {
            var rec = new DerivedFromDerivedSample();
            rec.RecordName = recordName;
            rec.RecordIndex = recordIndex;
            rec.DoubleElement = 300.0;
            rec.LocalDateElement = new LocalDate(2003, 5, 1);
            rec.LocalTimeElement = new LocalTime(10, 15, 30); // 10:15:30
            rec.LocalMinuteElement = new LocalMinute(10, 15); // 10:15
            rec.LocalDateTimeElement = new LocalDateTime(2003, 5, 1, 10, 15); // 2003-05-01T10:15:00
            rec.InstantElement = new LocalDateTime(2003, 5, 1, 10, 15).ToInstant(DateTimeZone.Utc); // 2003-05-01T10:15:00
            rec.OtherStringElement3 = String.Empty; // Test how empty value is recorded
            rec.OtherDoubleElement3 = 200.0;

            var dataSet = context.GetDataSet(dataSetName, context.DataSet);
            context.SaveOne(rec, dataSet);
            return rec.Id;
        }
    }
}
