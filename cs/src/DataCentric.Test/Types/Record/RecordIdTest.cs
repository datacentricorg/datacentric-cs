﻿/*
Copyright (C) 2013-present The DataCentric Authors.
Copyright (C) 2010-present MongoDB Inc.

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
using System.Linq;
using DataCentric;
using MongoDB.Bson; // TODO - remove the remaining use of MongoDB so RecordId is fully portable
using Xunit;

namespace DataCentric.Test
{
    public class RecordIdTest
    {
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void TestDateTimeConstructorAtEdgeOfRange(int secondsSinceEpoch)
        {
            var timestamp = BsonConstants.UnixEpoch.AddSeconds(secondsSinceEpoch);
            var objectId = new RecordId(timestamp, 0, 0, 0);
            Assert.Equal(timestamp, objectId.CreationTime);
        }

        [Theory]
        [InlineData((long)int.MinValue - 1)]
        [InlineData((long)int.MaxValue + 1)]
        public void TestDateTimeConstructorArgumentOutOfRangeException(long secondsSinceEpoch)
        {
            var timestamp = BsonConstants.UnixEpoch.AddSeconds(secondsSinceEpoch);
            Assert.Throws<ArgumentOutOfRangeException>(() => new RecordId(timestamp, 0, 0, 0));
        }

        [Fact]
        public void TestIComparable()
        {
            var objectId1 = RecordId.GenerateNewId();
            var objectId2 = RecordId.GenerateNewId();
            Assert.Equal(0, objectId1.CompareTo(objectId1));
            Assert.Equal(-1, objectId1.CompareTo(objectId2));
            Assert.Equal(1, objectId2.CompareTo(objectId1));
            Assert.Equal(0, objectId2.CompareTo(objectId2));
        }

        [Fact]
        public void TestCompareEqualGeneratedIds()
        {
            var objectId1 = RecordId.GenerateNewId();
            var objectId2 = objectId1;
            Assert.False(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.False(objectId1 != objectId2);
            Assert.True(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerTimestamp()
        {
            var objectId1 = new RecordId("0102030405060708090a0b0c");
            var objectId2 = new RecordId("0102030505060708090a0b0c");
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerMachine()
        {
            var objectId1 = new RecordId("0102030405060708090a0b0c");
            var objectId2 = new RecordId("0102030405060808090a0b0c");
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerPid()
        {
            var objectId1 = new RecordId("0102030405060708090a0b0c");
            var objectId2 = new RecordId("01020304050607080a0a0b0c");
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerIncrement()
        {
            var objectId1 = new RecordId("0102030405060708090a0b0c");
            var objectId2 = new RecordId("0102030405060708090a0b0d");
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareSmallerGeneratedId()
        {
            var objectId1 = RecordId.GenerateNewId();
            var objectId2 = RecordId.GenerateNewId();
            Assert.True(objectId1 < objectId2);
            Assert.True(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.False(objectId1 > objectId2);
            Assert.False(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerTimestamp()
        {
            var objectId1 = new RecordId("0102030405060708090a0b0c");
            var objectId2 = new RecordId("0102030305060708090a0b0c");
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerMachine()
        {
            var objectId1 = new RecordId("0102030405060808090a0b0c");
            var objectId2 = new RecordId("0102030405060708090a0b0c");
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerPid()
        {
            var objectId1 = new RecordId("01020304050607080a0a0b0c");
            var objectId2 = new RecordId("0102030405060708090a0b0c");
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerIncrement()
        {
            var objectId1 = new RecordId("0102030405060708090a0b0d");
            var objectId2 = new RecordId("0102030405060708090a0b0c");
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestCompareLargerGeneratedId()
        {
            var objectId2 = RecordId.GenerateNewId(); // generate before objectId2
            var objectId1 = RecordId.GenerateNewId();
            Assert.False(objectId1 < objectId2);
            Assert.False(objectId1 <= objectId2);
            Assert.True(objectId1 != objectId2);
            Assert.False(objectId1 == objectId2);
            Assert.True(objectId1 > objectId2);
            Assert.True(objectId1 >= objectId2);
        }

        [Fact]
        public void TestParse()
        {
            var objectId1 = RecordId.Parse("0102030405060708090a0b0c"); // lower case
            var objectId2 = RecordId.Parse("0102030405060708090A0B0C"); // upper case
            Assert.True(objectId1.ToByteArray().SequenceEqual(objectId2.ToByteArray()));
            Assert.True(objectId1.ToString() == "0102030405060708090a0b0c"); // ToString returns lower case
            Assert.True(objectId1.ToString() == objectId2.ToString());
            Assert.Throws<FormatException>(() => RecordId.Parse("102030405060708090a0b0c")); // too short
            Assert.Throws<FormatException>(() => RecordId.Parse("x102030405060708090a0b0c")); // invalid character
            Assert.Throws<FormatException>(() => RecordId.Parse("00102030405060708090a0b0c")); // too long
        }

        [Fact]
        public void TestTryParse()
        {
            RecordId objectId1, objectId2;
            Assert.True(RecordId.TryParse("0102030405060708090a0b0c", out objectId1)); // lower case
            Assert.True(RecordId.TryParse("0102030405060708090A0B0C", out objectId2)); // upper case
            Assert.True(objectId1.ToByteArray().SequenceEqual(objectId2.ToByteArray()));
            Assert.True(objectId1.ToString() == "0102030405060708090a0b0c"); // ToString returns lower case
            Assert.True(objectId1.ToString() == objectId2.ToString());
            Assert.False(RecordId.TryParse("102030405060708090a0b0c", out objectId1)); // too short
            Assert.False(RecordId.TryParse("x102030405060708090a0b0c", out objectId1)); // invalid character
            Assert.False(RecordId.TryParse("00102030405060708090a0b0c", out objectId1)); // too long
            Assert.False(RecordId.TryParse(null, out objectId1)); // should return false not throw ArgumentNullException
        }

        [Fact]
        public void TestConvertRecordIdToRecordId()
        {
            var oid = RecordId.GenerateNewId();

            var oidConverted = Convert.ChangeType(oid, typeof(RecordId));

            Assert.Equal(oid, oidConverted);
        }
    }
}