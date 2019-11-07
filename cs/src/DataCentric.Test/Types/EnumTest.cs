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
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Xunit;
using DataCentric;

namespace DataCentric.Test
{
    /// <summary>Enum type.</summary>
    public enum EnumTestSampleType
    {
        Empty,
        EnumValue1,
        EnumValue2,
        EnumValue3,
        EnumValue4,
        EnumValue5,
        EnumValue6,
        EnumValue7
    }

    /// <summary>Key class that has all of the permitted non-nullable key elements included.</summary>
    [BsonSerializer(typeof(BsonKeySerializer<EnumTestNonNullableSampleKey>))]
    public sealed class EnumTestNonNullableSampleKey : TypedKey<EnumTestNonNullableSampleKey, EnumTestNonNullableSample>
    {
        public EnumTestSampleType EnumAsKey { get; set; }
    }

    /// <summary>Key class that has all of the permitted non-nullable key elements included.</summary>
    public class EnumTestNonNullableSample : TypedRecord<EnumTestNonNullableSampleKey, EnumTestNonNullableSample>
    {
        public EnumTestSampleType EnumAsKey { get; set; }
        public EnumTestSampleType EnumValue { get; set; }
        public IsoDayOfWeek DayOfWeek { get; set; }
    }

    /// <summary>Key class that has all of the permitted nullable key elements included.</summary>
    [BsonSerializer(typeof(BsonKeySerializer<EnumTestNullableSampleKey>))]
    public sealed class EnumTestNullableSampleKey : TypedKey<EnumTestNullableSampleKey, EnumTestNullableSample>
    {
        public EnumTestSampleType? EnumAsKey { get; set; }
    }

    /// <summary>Key class that has all of the permitted nullable key elements included.</summary>
    public class EnumTestNullableSample : TypedRecord<EnumTestNullableSampleKey, EnumTestNullableSample>
    {
        public EnumTestSampleType? EnumAsKey { get; set; }
        public EnumTestSampleType? EnumValue { get; set; }
        public IsoDayOfWeek? DayOfWeek { get; set; }
    }

    /// <summary>Unit test for Query.</summary>
    public class EnumTest : UnitTest

    {
    /// <summary>
    /// Test that empty value is recognized by IsEmpty() method.
    ///
    /// One of the tests is for an enum with HasValue() defined,
    /// another for enum without HasValue().
    /// </summary>
    [Fact]
    public void EmptyValue()
    {
        using (var context = new UnitTestContext(this))
        {
            // Shows that we cannot detect special enum values such as None or Empty
            object none = IsoDayOfWeek.None;
            context.Log.Assert(none.IsEmpty() == false, "None.IsEmpty() == false");

            // Regular values are also not empty
            object mon = IsoDayOfWeek.Monday;
            context.Log.Assert(mon.IsEmpty() == false, "Monday.IsEmpty() == false");
        }
    }

    /// <summary>>Key class that has all of the permitted non-nullable key elements included.</summary>
    [Fact]
    public void CompleteNonNullableQuery()
    {
        using (var context = CreateMethodContext())
        {
            var records = new List<EnumTestNonNullableSample>();
            for (int recordIndex = 0; recordIndex < 8; ++recordIndex)
            {
                int recordIndexMod8 = recordIndex % 8;

                var record = new EnumTestNonNullableSample();
                record.EnumAsKey = (EnumTestSampleType) (recordIndexMod8);
                record.EnumValue = (EnumTestSampleType) (recordIndexMod8);
                record.DayOfWeek = (IsoDayOfWeek) recordIndexMod8;

                records.Add(record);
            }

            context.SaveMany(records);

            if (true)
            {
                // Query for all records without restrictions,
                // should return 8 records
                var query = context.DataSource.GetQuery<EnumTestNonNullableSample>(context.DataSet);

                context.Log.Verify("Unconstrained query");
                foreach (var obj in query.AsEnumerable())
                {
                    context.Log.Verify($"    Key={obj.Key} IsoDayOfWeek={obj.DayOfWeek}");
                }
            }

            if (true)
            {
                // Query for all records without restrictions,
                // should return 1 out of 8 records

                var query = context.DataSource.GetQuery<EnumTestNonNullableSample>(context.DataSet)
                    .Where(p => p.EnumAsKey == (EnumTestSampleType) 1)
                    .Where(p => p.EnumValue == (EnumTestSampleType) 1)
                    .Where(p => p.DayOfWeek == (IsoDayOfWeek) 1);

                context.Log.Verify("Constrained query");
                foreach (var obj in query.AsEnumerable())
                {
                    context.Log.Verify($"    Key={obj.Key} IsoDayOfWeek={obj.DayOfWeek}");
                }
            }
        }
    }

    /// <summary>>Key class that has all of the permitted nullable key elements included.</summary>
    [Fact]
    public void CompleteNullableQuery()
    {
        using (var context = CreateMethodContext())
        {
            var records = new List<EnumTestNullableSample>();
            for (int recordIndex = 0; recordIndex < 8; ++recordIndex)
            {
                int recordIndexMod8 = recordIndex % 8;

                var record = new EnumTestNullableSample();
                record.EnumAsKey = (EnumTestSampleType) (recordIndexMod8);
                record.EnumValue = (EnumTestSampleType) (recordIndexMod8);
                record.DayOfWeek = (IsoDayOfWeek) recordIndexMod8;

                records.Add(record);
            }

            context.SaveMany(records);

            if (true)
            {
                // Query for all records without restrictions,
                // should return 8 records
                var query = context.DataSource.GetQuery<EnumTestNullableSample>(context.DataSet);

                context.Log.Verify("Unconstrained query");
                foreach (var obj in query.AsEnumerable())
                {
                    context.Log.Verify($"    Key={obj.Key} IsoDayOfWeek={obj.DayOfWeek}");
                }
            }

            if (true)
            {
                // Query for all records without restrictions,
                // should return 1 out of 8 records

                var query = context.DataSource.GetQuery<EnumTestNullableSample>(context.DataSet)
                    .Where(p => p.EnumAsKey == (EnumTestSampleType) 1)
                    .Where(p => p.EnumValue == (EnumTestSampleType) 1)
                    .Where(p => p.DayOfWeek == (IsoDayOfWeek) 1);

                context.Log.Verify("Constrained query");
                foreach (var obj in query.AsEnumerable())
                {
                    context.Log.Verify($"    Key={obj.Key} IsoDayOfWeek={obj.DayOfWeek}");
                }
            }
        }
    }
    }
}
