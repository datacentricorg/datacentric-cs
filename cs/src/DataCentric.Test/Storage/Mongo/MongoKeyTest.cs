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
    /// <summary>Unit test for key serialization in Mongo 2.1 format.</summary>
    public class MongoKeyTest : UnitTest
    {
        /// <summary>>Key class that has all of the permitted non-nullable key elements included.</summary>
        [Fact]
        public void CompleteNonNullableKey()
        {
            using (var context = CreateMethodContext())
            {
                var record = new NonNullableElementsSample();
                record.DataSet = context.DataSet;
                record.StringToken = "A";
                record.BoolToken = true;
                record.IntToken = 123;
                record.LongToken = 12345678912345;
                record.LocalDateToken = new LocalDate(2003, 5, 1);
                record.LocalTimeToken = new LocalTime(10, 15, 30); // 10:15:30
                record.LocalMinuteToken = new LocalMinute(10, 15); // 10:15
                record.LocalDateTimeToken = new LocalDateTime(2003, 5, 1, 10, 15); // 2003-05-01T10:15:00
                record.InstantToken = new LocalDateTime(2003, 5, 1, 10, 15).ToInstant(DateTimeZone.Utc); // 2003-05-01T10:15:00
                record.EnumToken = SampleEnum.EnumValue2;

                // Verify key serialization
                context.Log.Verify(record.Key);

                // Verify key creation
                var key = record.ToKey();
                context.Log.Verify(key.Value);

                // Save
                context.SaveOne(record, context.DataSet);

                // Load from storage
                var loadedRecord = context.LoadOrNull(key, context.DataSet);
                context.Log.Verify(loadedRecord.Key);
            }
        }

        /// <summary>>Key class that has all of the permitted nullable key elements included.</summary>
        [Fact]
        public void CompleteNullableKey()
        {
            using (var context = CreateMethodContext())
            {
                var record = new NullableElementsSample();
                record.DataSet = context.DataSet;
                record.StringToken = "A";
                record.BoolToken = true;
                record.IntToken = 123;
                record.LongToken = 12345678912345;
                record.LocalDateToken = new LocalDate(2003, 5, 1);
                record.LocalTimeToken = new LocalTime(10, 15, 30); // 10:15:30
                record.LocalMinuteToken = new LocalMinute(10, 15); // 10:15
                record.LocalDateTimeToken = new LocalDateTime(2003, 5, 1, 10, 15); // 2003-05-01T10:15:00
                record.InstantToken = new LocalDateTime(2003, 5, 1, 10, 15).ToInstant(DateTimeZone.Utc); // 2003-05-01T10:15:00
                record.EnumToken = SampleEnum.EnumValue2;

                // Verify key serialization
                context.Log.Verify(record.Key);

                // Verify key creation
                var key = record.ToKey();
                context.Log.Verify(key.Value);

                // Save
                context.SaveOne(record, context.DataSet);

                // Load from storage
                var loadedRecord = context.LoadOrNull(key, context.DataSet);
                context.Log.Verify(loadedRecord.Key);
            }
        }

        /// <summary>
        /// Test composite key that has an embedded key with more than one token.
        /// </summary>
        [Fact]
        public void CompositeKey()
        {
            using (var context = CreateMethodContext())
            {
                context.KeepTestData();

                var rec = new CompositeKeySample();
                rec.KeyElement1 = "abc";
                rec.KeyElement2 = new BaseSampleKey();
                rec.KeyElement2.RecordName = "def";
                rec.KeyElement2.RecordIndex = 123;
                rec.KeyElement3 = "xyz";

                // Verify key serialization
                string keyValue = rec.ToKey().ToString();
                context.Log.Verify($"Serialized key: {keyValue}");

                // Verify key deserialization
                var key = new CompositeKeySampleKey();
                key.PopulateFrom(keyValue);
                context.Log.Verify($"Deserialized key: {key}");
            }
        }

        /// <summary>
        /// Test empty key for a singleton.
        /// </summary>
        [Fact]
        public void SingletonKey()
        {
            using (var context = CreateMethodContext())
            {
                context.KeepTestData();

                var rec = new SingletonSample();
                rec.StringElement = "abc";

                // Verify key serialization
                string keyValue = rec.ToKey().ToString();
                context.Log.Assert(keyValue == String.Empty, "Serialized key for a singleton must be String.Empty assert.");

                // Verify key deserialization
                var key = new SingletonSampleKey();
                key.PopulateFrom(keyValue);
                context.Log.Assert(key.ToString() == String.Empty, "Deserialized key for a singleton must be String.Empty assert.");
            }
        }

        /// <summary>
        /// Test key based on the record's Id.
        /// </summary>
        [Fact]
        public void IdBasedKey()
        {
            using (var context = CreateMethodContext())
            {
                context.KeepTestData();

                // Create from timestamp
                var createdTime = InstantUtil.Utc(2003, 5, 1, 10, 15, 0);
                var rec = new IdBasedKeySample();
                rec.Id = new TemporalId(createdTime, 1, 2, 3);
                rec.StringElement = "abc";

                // Verify key serialization
                string keyValue = rec.ToKey().ToString();
                context.Log.Verify($"Serialized key: {keyValue}");

                // Verify key deserialization
                var key = new IdBasedKeySampleKey();
                key.PopulateFrom(keyValue);
                context.Log.Verify($"Deserialized key: {key}");

                context.Log.Assert(key.Id == rec.Id, "TemporalId serialization roundtrip.");
            }
        }
    }
}
